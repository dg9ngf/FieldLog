﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Unclassified.Util;
using System.IO.Compression;

namespace PdbConvert
{
	internal class Program
	{
		private static int Main()
		{
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
			ConsoleHelper.FixEncoding();

			List<string> assemblyFileNames = new List<string>();

			CommandLineHelper cmdLine = new CommandLineHelper();
			var gzOption = cmdLine.RegisterOption("gz");
			var helpOption = cmdLine.RegisterOption("help").Alias("h", "?");
			var noNamesOption = cmdLine.RegisterOption("nonames");
			var optimizeOption = cmdLine.RegisterOption("optimize");
			var outFileOption = cmdLine.RegisterOption("outfile", 1).Alias("o");
			var srcBaseOption = cmdLine.RegisterOption("srcbase", 1);

			try
			{
				cmdLine.Parse();
			}
			catch (Exception ex)
			{
				return ConsoleHelper.ExitError(ex.Message, 1);
			}

			foreach (var arg in cmdLine.Arguments)
			{
				if (arg.Option == helpOption)
				{
					ShowHelp();
					return 0;
				}
				else if (arg.Option == null)
				{
					if (arg.Value.IndexOfAny(new[] { '?', '*' }) != -1)
					{
						foreach (string foundFile in Directory.GetFiles(Path.GetDirectoryName(arg.Value), Path.GetFileName(arg.Value)))
						{
							CheckAddAssemblyFile(assemblyFileNames, foundFile);
						}
					}
					else
					{
						CheckAddAssemblyFile(assemblyFileNames, arg.Value);
					}
				}
			}

			if (assemblyFileNames.Count == 0)
			{
				return ConsoleHelper.ExitError("Missing argument: Input assembly file name", 2);
			}

			XmlDocument doc = new XmlDocument();
			XmlWriter xw = doc.CreateNavigator().AppendChild();
			PdbReader reader = new PdbReader(assemblyFileNames, xw);
			if (srcBaseOption.IsSet)
			{
				reader.SourceBasePath = Path.GetFullPath(srcBaseOption.Value);
			}
			try
			{
				reader.Convert();
			}
			catch (Exception ex)
			{
				return ConsoleHelper.ExitError("Cannot convert symbols. " + ex.Message, 3);
			}
			xw.Close();

			if (optimizeOption.IsSet)
			{
				// Reduce multiple subsequent "hidden" sequence points to the first of them
				doc.SelectNodes("/symbols/module/methods/method/sequencePoints/entry[@hidden='true'][preceding-sibling::entry[1][@hidden='true']]")
					.OfType<XmlNode>()
					.ForEachSafe(n => n.ParentNode.RemoveChild(n));

				// Remove sequence points if they are all hidden
				doc.SelectNodes("/symbols/module/methods/method/sequencePoints[not(entry[@startLine])]")
					.OfType<XmlNode>()
					.ForEachSafe(n => n.ParentNode.RemoveChild(n));

				// Remove all methods with no sequence points
				doc.SelectNodes("/symbols/module/methods/method[not(sequencePoints)]")
					.OfType<XmlNode>()
					.ForEachSafe(n => n.ParentNode.RemoveChild(n));

				// Remove all files with no remaining methods
				doc.SelectNodes("/symbols/module/files/file")
					.OfType<XmlNode>()
					.ForEachSafe(n =>
					{
						if (n.SelectNodes("../../methods/method/sequencePoints/entry[@fileRef='" + n.Attributes["id"].Value + "']").Count == 0)
						{
							n.ParentNode.RemoveChild(n);
						}
					});
			}

			if (noNamesOption.IsSet)
			{
				// Remove all name attributes from methods
				doc.SelectNodes("/symbols/module/methods/method")
					.OfType<XmlNode>()
					.ForEachSafe(n => n.Attributes.RemoveNamedItem("name"));
			}

			string xmlFileName =
				outFileOption.Value ??
				assemblyFileNames.OrderBy(a => Path.GetExtension(a).Equals(".dll", StringComparison.OrdinalIgnoreCase)).First() + ".xml" +
					(gzOption.IsSet ? ".gz" : "");
			try
			{
				if (gzOption.IsSet)
				{
					using (FileStream fileStream = new FileStream(xmlFileName, FileMode.Create))
					using (GZipStream gzStream = new GZipStream(fileStream, CompressionMode.Compress))
					{
						doc.Save(gzStream);
					}
				}
				else
				{
					doc.Save(xmlFileName);
				}
			}
			catch (Exception ex)
			{
				return ConsoleHelper.ExitError("Cannot save XML file. " + ex.Message, 4);
			}
			return 0;
		}

		private static void CheckAddAssemblyFile(List<string> assemblyFileNames, string fileName)
		{
			if ((Path.GetExtension(fileName) == ".exe" || Path.GetExtension(fileName) == ".dll") &&
				File.Exists(Path.ChangeExtension(fileName, "pdb")))
			{
				assemblyFileNames.Add(Path.GetFullPath(fileName));
			}
		}

		private static void ShowHelp()
		{
			ConsoleHelper.WriteLine("PdbConvert", ConsoleColor.White);
			ConsoleHelper.WriteWrapped(
@"Converts .pdb debug symbols to a portable XML file.
Part of FieldLog, © Yves Goergen, GNU GPL v3
Website: http://dev.unclassified.de/source/fieldlog

Usage: PdbConvert [options] inputfile ...

The input files must be either .exe or .dll assembly files. Only assemblies with an existing .pdb file are processed. Wildcards (? and *) are allowed for files within a directory. All files’ symbols are written to one XML file. Options and input files can be mixed.

Options:
  /gz            Compresses the output file with gzip. (Default file extension: .xml.gz)
  /nonames       Does not include method names in the XML file.
  /optimize      Removes unnecessary elements (methods without source code, unreferenced source files) from the XML file.
  /outfile       Specifies the path of the generated XML file. If unset, the file is created in the path of the input files. A .exe input file is considered first.
  /srcbase       Sets the source base path that is stripped from the beginning of the source file names. Set this to the project directory.
  /help, /h, /?  Shows this help information.", true);
		}
	}
}
