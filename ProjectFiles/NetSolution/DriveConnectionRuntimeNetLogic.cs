#region Using directives
using UAManagedCore;
using FTOptix.NetLogic;
using CgpZmqApplication;
using DirectDriveConnectivity;
using System;
using System.Collections.Concurrent;
using System.Threading;
using PFCodes = System.Collections.Generic.List<DirectDriveConnectivity.IdentityCode>;
using System.Linq;
#endregion

namespace DirectDriveConnectivity
{
    public class DeviceActions
    {
        public PFCodes PfCodes { get; set; }
        public Ia.ActionData IdentityAction { get; set; }
        public Ia.ActionData StartActions { get; set; }
        public Ia.ActionData StopActions { get; set; }
    }
}

public class DriveConnectionRuntimeNetLogic : BaseNetLogic
{
    public override void Start()
    {
        if (cgpProcess.StartCgp())
        {
            Log.Info(FTOptixLog.DDCCategory, "CGP executable started");
        }
        else
        {
            throw new CoreConfigurationException("CGP start failed");
        }

        dataForOptix = new BlockingCollection<NodeValuePair>();
        dataForOptixCancelTokenSource = new CancellationTokenSource();

        var modelUtils = new ModelUtils(LogicObject);
        var folders = modelUtils.GetDataPointModelFolders();
        var actions = new ConcurrentBag<DeviceActions>();
        var uaNodes = new ConcurrentDictionary<string, IUANode>();
        foreach (var folder in folders)
        {
            var deviceActions = CreateDeviceActions(folder, uaNodes);
            if (deviceActions != null)
            {
                actions.Add(deviceActions);
            }
        }

        application = new CgpApplication(actions, uaNodes);
        RegisterCgpApplication();
        application.StopActionsAvailable += AddStopActions;
        pushDataToOptixTask = new LongRunningTask(PushDataToOptix, LogicObject);
        pushDataToOptixTask.Start();

        foreach (var action in actions.Select(action => action.IdentityAction))
        {
            application.SendRequest(action);
        }
    }

    private DeviceActions CreateDeviceActions(IUANode folder, ConcurrentDictionary<string, IUANode> uaNodes)
    {
        try
        {
            var tags = folder.GetObject(ModelUtils.Tags);
            var updateRate = folder.GetVariable(ModelUtils.UpdateRateString);
            var ipAddress = folder.GetVariable(ModelUtils.IPAddress);
            var powerFlexType = folder.GetVariable(ModelUtils.PowerFlexType);
            if (tags == null || updateRate == null || ipAddress == null || powerFlexType == null)
            {
                throw new CoreConfigurationException(string.Format("Folder {0} will not be processed due to incomplete structure.", folder.BrowseName));
            }

            var dataPoints = cdaRequest.ExtractDataPointsFromModel(tags);

            foreach (var dataPointFromModel in tags.Children)
            {
                var dataPointName = cdaRequest.GetDataPointName(dataPointFromModel);
                if (!uaNodes.TryAdd(dataPointName, dataPointFromModel))
                {
                    var message = "Data point {0} has already been added to Direct Drive Connectivity nodes collection.";
                    Log.Warning(FTOptixLog.DDCCategory, string.Format(message, dataPointName));
                }
            }

            var actionsForSingleFolder = cdaRequest.CreateUniqCdaActions(dataPoints, updateRate.Value);

            var identityAction = cdaRequest.PrepareIdentityAction(cdaRequest.CreateIdentityRequestUri(ipAddress.Value),
                folder.BrowseName);
            (var startActions, var stopActions) = cdaRequest.PrepareCdaActions(actionsForSingleFolder);
            return new DeviceActions()
            {
                IdentityAction = identityAction,
                PfCodes = GetPFCodes(powerFlexType.Value),
                StartActions = startActions,
                StopActions = stopActions
            };
        }
        catch (Exception ex)
        {
            Log.Error(FTOptixLog.DDCCategory, string.Format("Error during processing folder {0}: {1}.", folder.BrowseName, ex.Message));
            return null;
        }
    }

    private void RegisterCgpApplication()
    {
        var applicationId = Guid.NewGuid().ToString();
        applicationComm = new ApplicationComm($"{{ 'name': 'DriveConnectApp_{applicationId}', 'ip': '127.0.0.1', 'port': '50005' }}");
        application.instrument = applicationComm;
    }

    private PFCodes GetPFCodes(string powerFlexType)
    {
        var codes = PFCodesCreator.GetCodes(powerFlexType);
        if (!codes.Any())
        {
            Log.Warning(FTOptixLog.DDCCategory, string.Format("No identity codes for Power Flex type {0} available.", powerFlexType));
        }

        return codes;
    }

    private void AddStopActions(object sender, StopActionsAvailableEventArgs e)
    {
        allStopActions.Add(e.data);
    }

    private void PushDataToOptix()
    {
        try
        {
            application.DataForOptixReady += DataForOptixReady;
            while (true)
            {
                var nodeData = dataForOptix.Take(dataForOptixCancelTokenSource.Token);
                var node = nodeData.Node as IUAVariable;
                node.DataValue = nodeData.DataValue;
            }
        }
        catch (OperationCanceledException)
        {
            // operation was cancelled
        }
        catch (Exception ex)
        {
            Log.Error(FTOptixLog.DDCCategory, string.Format("Exception during pushing data to FT Optix variable: {0}.", ex.Message));
        }
        finally
        {
            application.DataForOptixReady -= DataForOptixReady;
        }
    }

    public override void Stop()
    {
        try
        {
            foreach (var action in allStopActions)
            {
                application.SendRequest(action);
            }
            application.StopActionsAvailable -= AddStopActions;
            allStopActions.Clear();
            dataForOptixCancelTokenSource?.Cancel();
            pushDataToOptixTask?.Cancel();
            dataForOptixCancelTokenSource?.Dispose();
            pushDataToOptixTask?.Dispose();
            dataForOptix?.Dispose();
            applicationComm?.Stop();
            application = null;
            applicationComm = null;
            cgpProcess?.StopCgp();
        }
        catch (Exception ex)
        {
            Log.Error(FTOptixLog.DDCCategory, string.Format("Exception during clean up activities: {0}.", ex.Message));
        }
    }

    protected virtual void DataForOptixReady(object sender, DataForOptixReadyEventArgs e)
    {
        try
        {
            foreach (var dataTuple in e.data)
            {
                dataForOptix.Add(dataTuple, dataForOptixCancelTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // operation was cancelled
        }
        catch (ObjectDisposedException ex)
        {
            Log.Info(FTOptixLog.DDCCategory, string.Format("Exception during data preparation: {0}.", ex.Message));
        }
        catch (Exception ex)
        {
            Log.Error(FTOptixLog.DDCCategory, string.Format("Exception during data preparation: {0}.", ex.Message));
        }
    }

    private CgpApplication application;
    private ApplicationComm applicationComm;
    private readonly CdaRequest cdaRequest = new();
    private readonly CgpProcess cgpProcess = new();
    private BlockingCollection<NodeValuePair> dataForOptix = null;
    private CancellationTokenSource dataForOptixCancelTokenSource = null;
    private LongRunningTask pushDataToOptixTask = null;
    private readonly ConcurrentBag<Ia.ActionData> allStopActions = new();
}

