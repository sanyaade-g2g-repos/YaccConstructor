using System.Windows.Forms;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.UI.ToolWindowManagement;
using JetBrains.UI.Application;
using Microsoft.VisualStudio.Shell.Interop;
using JetBrains.ReSharper.Daemon;
using JetBrains.TextControl;
using JetBrains.Application;
using System;
using JetBrains.ReSharper.Features.Altering.Resources;
using GraphX;
using GraphX.GraphSharp.Algorithms.Layout.Simple.FDP;
using System.Windows.Controls;
using GraphX.Controls;
using YC.ReSharper.AbstractAnalysis.Plugin.Highlighting;

namespace YC.ReSharper.AbstractAnalysis.Plugin
{

    #region Descriptor
    [ToolWindowDescriptor(
    ProductNeutralId = "ClassName",
    Text = "Graph of code",
    VisibilityPersistenceScope = ToolWindowVisibilityPersistenceScope.Solution,
    Icon = typeof(AlteringFeatuThemedIcons.GeneratedMembers),
    Type = ToolWindowType.SingleInstance,
    InitialDocking = ToolWindowInitialDocking.NotSpecified)]
    public class WindowDescriptor : ToolWindowDescriptor
    {
        public WindowDescriptor(IApplicationDescriptor applicationDescriptor)
            : base(applicationDescriptor)
        {
        }
    }
    #endregion
    [ActionHandler("Plugin.ToolWindow.About")]
    public class WindowAction : IActionHandler
    {
        private readonly IDaemonProcess myDaemonProcess;
        private WindowDescriptor descriptor;
        private ToolWindowManager manager;
        private Lifetime lifetime;
        private UIApplication uiApplication;
        private IVsUIShell shell;
        private WindowRegistrar registrar;
        //private ISolution solution;
        //private ITextControl codefile;
        //public IDocument document;
        public static ITextControl textControl;
        //public static IDeclaredElement element;
        public static AreaControl arcont;


        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            return true;
        }
        public static void GoToCode(ITextControl t, Edge e)
        {
            var p = t.Caret.PositionValue;
            t.Caret.MoveTo(new DocOffsetAndVirtual(736), new CaretVisualPlacement());
        }

        public static void GoToGraph(object sender, EventArgs args)
        {
            MessageBox.Show("asdf");
        }

        public static void ControlsForGraphs()
        {
            int a = Handler.DataGraphs.Count - WindowAction.arcont.EControl.Items.Count;
            while (a > 0)
            {
                //arcont.DataGraphs.Count - a
                int pos = Handler.DataGraphs.Count - a;
                var logicCore = new LogicCore() { Graph = (Graph)Handler.DataGraphs[pos] };
                logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
                logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.LinLog);
                ((LinLogLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).IterationCount = 100;
                logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
                logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
                logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;
                logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
                logicCore.AsyncAlgorithmCompute = false;
                arcont.EControl.Items.Add(new TabItem()
                {
                    Header = "Graph" + (pos + 1).ToString(),
                    Content = new ZoomControl() { Content = new EmptyGraphArea() { LogicCore = logicCore } }
                });
                //arcont.EControl.Items.Add(WindowAction.arcont.DataGraphs[0]);

                a--;
            }
        }

        public static void AddTabControls(object sender, EventArgs e)
        {

            arcont.EControl.Items.Add(new TabItem());
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            //TODO same method

            try
            {
                //solution = DataConstantsExtensions.GetComponent<ISolution>(context);
                textControl = context.GetData(JetBrains.TextControl.DataContext.DataConstants.TEXT_CONTROL);
                //TODO mere beayty analiser
                //TextControl.Caret.CaretMoved += GoToGraph;
                if (descriptor == null || manager == null || lifetime == null || uiApplication == null || shell == null ||
                registrar == null)
                {
                    arcont = new AreaControl();
                    //arcont.Area.SetVerticesMathShape(VertexShape.Circle);
                    arcont.Graph_Setup();
                    arcont.GraphArea_Setup();
                    arcont.Loaded += arcont.MainWindow_Loaded;
                    descriptor = DataConstantsExtensions.GetComponent<WindowDescriptor>(context);
                    manager = DataConstantsExtensions.GetComponent<ToolWindowManager>(context);
                    lifetime = DataConstantsExtensions.GetComponent<Lifetime>(context);
                    uiApplication = DataConstantsExtensions.GetComponent<UIApplication>(context);
                    shell = DataConstantsExtensions.GetComponent<IVsUIShell>(context);
                    registrar = new WindowRegistrar(lifetime, manager, shell, descriptor, uiApplication);

                }
                registrar.Show();
            }
            catch (Exception)
            {
                MessageBox.Show("Please, open project");
            }
        }
    }

    public class WindowRegistrar
    {
        private readonly Lifetime lifetime;
        private readonly ToolWindowClass toolWindowClass;
        private readonly UIApplication environment;
        private ToolWindowInstance instance;

        public WindowRegistrar(Lifetime lifetime, ToolWindowManager toolWindowManager, IVsUIShell shell,
            WindowDescriptor descriptor, UIApplication environment)
        {
            this.environment = environment;
            this.lifetime = lifetime;
            this.toolWindowClass = toolWindowManager.Classes[descriptor];
            instance = this.toolWindowClass.RegisterInstance(
            this.lifetime,
            "", // title of your window; tip: StringUtil.MakeTitle
            null, // return a System.Drawing.Image to be displayed
            (lt, twi) =>
            {

                return WindowAction.arcont;
            });
            instance.EnsureControlCreated().Show();
        }

        public void Show()
        {
            if (WindowAction.arcont != null)
            {
                WindowAction.ControlsForGraphs();

                WindowAction.arcont.gg_but_randomgraph_Click(null, null);
            }

            instance.EnsureControlCreated().Show();
        }
    }
}