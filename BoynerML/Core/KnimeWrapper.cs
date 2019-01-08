using KnimeNet.CommandLine;
using KnimeNet.CommandLine.Types;
using KnimeNet.CommandLine.Types.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BoynerML.Core
{
    public class KnimeWrapper
    {
        private const string FailingWorkFlow = @"C:\ecelik\Boyner\Knime\Results\Failed";
        private const string SucceedingWorkFlow = @"C:\ecelik\Boyner\Knime";
        private const string KnimeDir = @"C:\Program Files\KNIME";

        /// <summary>
        /// This example executes a workflow that ends without errors without binding of output/error.
        /// </summary>
        /// <returns></returns>
        public static async Task<ExitStatus> SucceedingExecutionWithoutBinding()
        {
            var output = new EventBasedTextWriter();
            var error = new EventBasedTextWriter();

            output.OnWriteLine += OnOutputReceived;
            error.OnWriteLine += OnErrorReceived;
            var shellProxy = new ShellProxy(KnimeDir)
            {
                Arguments =
                {
                    WorkFlowDir = SucceedingWorkFlow
                }
            };
            var exitStatus = await shellProxy.StartKnime();
            return exitStatus;
        }

        /// <summary>
        /// This example executes a workflow that ends without errors and binds the output/error to event based textwriters.
        /// </summary>
        /// <returns></returns>
        public static async Task<ExitStatus> SucceedingExecutionWithEventBasedBinding()
        {
            var output = new EventBasedTextWriter();
            var error = new EventBasedTextWriter();

            output.OnWriteLine += OnOutputReceived;
            error.OnWriteLine += OnErrorReceived;
            var shellProxy = new ShellProxy(KnimeDir)
            {
                Arguments =
                {
                    WorkFlowDir = SucceedingWorkFlow
                }
            };
            var exitStatus = await shellProxy.StartKnime(null, output, error);
            return exitStatus;
        }

        /// <summary>
        /// This example executes a workflow that ends without errors and binds the output/errors to Console.
        /// </summary>
        /// <returns></returns>
        public static async Task<ExitStatus> SucceedingExecutionWithDifferentTextWriter()
        {
            var shellProxy = new ShellProxy(KnimeDir)
            {
                Arguments =
                {
                    WorkFlowDir = SucceedingWorkFlow
                }
            };
            var exitStatus = await shellProxy.StartKnime(null, Console.Out, Console.Error);
            return exitStatus;
        }

        /// <summary>
        /// This method executes a workflow that will fail. A popup is forced so KNIME blocks.
        /// After a timeout is reached KNIME will be killed.
        /// </summary>
        /// <returns></returns>
        public static async Task<ExitStatus> FailingExecutionAfterTimeout()
        {
            var output = new EventBasedTextWriter();
            var error = new EventBasedTextWriter();

            output.OnWriteLine += OnOutputReceived;
            error.OnWriteLine += OnErrorReceived;
            var shellProxy = new ShellProxy(KnimeDir)
            {
                Arguments =
                {
                    WorkFlowDir = FailingWorkFlow,
                    SuppressErrors = false // provoke blocking KNIME instance in case the workflow ends too fast
                },
                KillOnError = true,
                Timeout = 20
            };
            var exitStatus = await shellProxy.StartKnime(null, output, error).ConfigureAwait(false);
            Console.WriteLine("Killed KNIME successfully? " + exitStatus.KilledProcess);
            return exitStatus;
        }

        /// <summary>
        /// This method executes a workflow that will fail. A popup is forced so KNIME blocks.
        /// A second thread is spawned that will abort the execution and kill KNIME.
        /// </summary>
        /// <returns></returns>
        public static async Task<ExitStatus> FailingExecutionAfterCancel()
        {
            var output = new EventBasedTextWriter();
            var error = new EventBasedTextWriter();

            output.OnWriteLine += OnOutputReceived;
            error.OnWriteLine += OnErrorReceived;
            var shellProxy = new ShellProxy(KnimeDir)
            {
                Arguments =
                {
                    WorkFlowDir = FailingWorkFlow,
                    SuppressErrors = false // provoke blocking KNIME instance in case the workflow ends too fast
                },
                KillOnError = true,
            };
#pragma warning disable 4014
            Task.Run(() => CancelKnime(shellProxy)); // parallel cancel helper
#pragma warning restore 4014
            var exitStatus = await shellProxy.StartKnime(null, output, error).ConfigureAwait(false);
            Console.WriteLine("Killed KNIME successfully? " + exitStatus.KilledProcess);
            return exitStatus;
        }

        private static async Task CancelKnime(ShellProxy proxy)
        {
            await Task.Delay(30000);
            proxy.AbortKnime();
        }

        /* Below are two event handlers that receive KNIME data. In the example we only log the data but you could obviously react different
         * based on the data content you receive */

        private static void OnOutputReceived(object sender, TextWriterEventArgs e)
        {
            /* receive the outputs from KNIME */
            Console.WriteLine("Output: " + e.Data);
        }
        private static void OnErrorReceived(object sender, TextWriterEventArgs e)
        {
            /* receive the errors from KNIME */
            Console.WriteLine("Error: " + e.Data);
        }
    }
}