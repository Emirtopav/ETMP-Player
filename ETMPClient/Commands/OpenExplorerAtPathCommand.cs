using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace ETMPClient.Commands
{
    public class OpenExplorerAtPathCommand : CommandBase
    {
        public OpenExplorerAtPathCommand()
        {

        }

        public override void Execute(object? parameter)
        {
            if(parameter is string path)
            {
                string argument = "/select, \"" + path + "\"";

                Process.Start("explorer.exe", argument);

            }
            else
            {
                string what = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? AppDomain.CurrentDomain.BaseDirectory)+"\\downloads";
                Process.Start("explorer.exe",what);
            }
        }
    }
}
