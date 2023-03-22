using Sandbox.ModAPI.Ingame;
using System.Collections;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public interface IApplication
        {
            void Build(IAgent agent);
            void Initialize();
            void Run();
            void OnClose();
            void OnClosing();

            Window MainWindow { get; }
            List<Window> Windows { get; }
        }
    }
}

