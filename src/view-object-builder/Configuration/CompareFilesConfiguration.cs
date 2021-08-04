using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

namespace viewObjectBuilder.Configuration
{
    [HelpOption,
     Command(Description = "Compares two files and returns 0 if the same, 1 if differences are detected.")]
    public class CompareFilesConfiguration : IVerbose
    {
        [Required,
         Option("-f|--File <Path1> -f|--File <Path2>", "Files to be compared. Two and only two may be specified.",
             CommandOptionType.MultipleValue)]
        public string[] Files { get; set; }

        [Option("-v|--Verbose",
            "Extended/detailed output messaging. Defaults to false.",
            CommandOptionType.SingleValue)]
        public bool Verbose { get; }

        public int OnExecute()
        {
            var resultCode = 1;
            var resultMessages = new List<string>();

            try
            {
                Configuration.Verbose = Verbose;
                if (Files?.Length != 2)
                {
                    throw new ValidationException("Two and only two files may be specified for comparison.");
                }

                var theSame = IsTheSame(Files[0], Files[1]);
                if (theSame)
                {
                    resultCode = 0;
                    resultMessages.Add($"File one ({Files[0]}) and file 2 ({Files[1]}) are equivalent.");
                }
                else
                {
                    resultMessages.Add($"File one ({Files[0]}) and file 2 ({Files[1]}) contain differences.");
                }
            }
            catch (Exception exc)
            {
                resultMessages.Add(Files?.Length == 2
                    ? $"Error comparing files '{Files[0]}' to '{Files[1]}'"
                    : "Error comparing files");

                resultMessages.Add(exc.ToString());
            }
            finally
            {
                Configuration.ProcessResults = resultMessages;
            }

            return resultCode;
        }

        public bool IsTheSame(string filePath1, string filePath2)
        {
            var reader = new StreamReader(filePath1);

            var file1 = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();

            reader = new StreamReader(filePath2);
            var file2 = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();

            return !Extensions.StringLineDifference(file1, file2);
        }
    }
}