using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HAComputerGateway.Win7.Other
{
    public class NLogTextWriter : TextWriter
    {
        private readonly TextWriter originalWriter;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public NLogTextWriter(TextWriter original)
        {
            originalWriter = original;
        }

        public override Encoding Encoding => originalWriter.Encoding;

        public override void Write(string value)
        {
            // 写到原始控制台
            originalWriter.Write(value);
            // 写入 NLog 日志（可以根据需要选择日志级别）
            Logger.Info(value);
        }

        public override void WriteLine(string value)
        {
            originalWriter.WriteLine(value);
            Logger.Info(value);
        }
    }
}
