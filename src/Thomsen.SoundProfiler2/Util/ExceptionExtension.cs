using System;
using System.Text;

namespace Thomsen.WpfTools.Util {
    public static class ExceptionExtension {
        public static string GetAllMessages(this Exception ex) {
            StringBuilder sb = new();

            sb.AppendLine($"{ex.Message}");
            Exception? innerEx = ex.InnerException;

            for (int ii = 0; innerEx is not null; ii++) {
                sb.AppendLine($"{new string('-', ii + 1)}> {innerEx.Message}");
                innerEx = innerEx.InnerException;
            }

            return sb.ToString();
        }
    }
}
