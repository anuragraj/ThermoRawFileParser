﻿using System;
using log4net;
using log4net.Core;
using Mono.Options;
using ThermoFisher.CommonCore.Data;

namespace ThermoRawFileParser
{
    public static class MainClass
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            string rawFilePath = null;
            string outputDirectory = null;
            string outputFormatString = null;
            var outputFormat = OutputFormat.NONE;
            var gzip = false;
            string outputMetadataString = null;
            var outputMetadataFormat = MetadataFormat.NONE;
            string s3url = null;
            string s3AccessKeyId = null;
            string s3SecretAccessKey = null;
            var verbose = false;
            string bucketName = null;
            var ignoreInstrumentErrors = false;

            var help = false;

            var optionSet = new OptionSet
            {
                {
                    "h|help", "Prints out the options.",
                    h => help = h != null
                },
                {
                    "i=|input=", "The raw file input.",
                    v => rawFilePath = v
                },
                {
                    "o=|output=", "The output directory.",
                    v => outputDirectory = v
                },
                {
                    "f=|format=",
                    "The output format for the spectra (0 for MGF, 1 for mzMl, 2 for indexed mzML, 3 for Parquet, 4 for MGF with profile data excluded)",
                    v => outputFormatString = v
                },
                {
                    "m=|metadata=", "The metadata output format (0 for JSON, 1 for TXT).",
                    v => outputMetadataString = v
                },
                {
                    "g|gzip", "GZip the output file if this flag is specified (without value).",
                    v => gzip = v != null
                },
                {
                    "u:|s3_url:",
                    "Optional property to write directly the data into S3 Storage.",
                    v => s3url = v
                },
                {
                    "k:|s3_accesskeyid:",
                    "Optional key for the S3 bucket to write the file output.",
                    v => s3AccessKeyId = v
                },
                {
                    "t:|s3_secretaccesskey:",
                    "Optional key for the S3 bucket to write the file output.",
                    v => s3SecretAccessKey = v
                },
                {
                    "n:|s3_bucketName:",
                    "S3 bucket name",
                    v => bucketName = v
                },
                {
                    "v|verbose", "Enable verbose logging.",
                    v => verbose = v != null
                },
                {
                    "e|ignoreInstrumentErrors", "Ignore missing properties by the instrument.",
                    v => ignoreInstrumentErrors = v != null
                }
            };

            try
            {
                // parse the command line
                var extra = optionSet.Parse(args);

                if (!extra.IsNullOrEmpty())
                {
                    throw new OptionException("unexpected extra arguments", null);
                }

                if (help)
                {
                    ShowHelp(" usage is (use -option=value for the optional arguments):", null,
                        optionSet);
                    return;
                }

                if (outputMetadataString == null && outputFormatString == null)
                {
                    throw new OptionException("The parameter -f or -m should be provided",
                        "-f|--format , -m|--format");
                }

                if (outputFormatString != null)
                {
                    int outPutFormatInt;
                    try
                    {
                        outPutFormatInt = int.Parse(outputFormatString);
                    }
                    catch (FormatException e)
                    {
                        throw new OptionException(
                            "unknown output format value (0 for MGF, 1 for mzMl, 2 for indexed mzML, 3 for Parquet, 4 for MGF with profile date excluded)",
                            "-f, --format");
                    }

                    if (Enum.IsDefined(typeof(OutputFormat), outPutFormatInt) &&
                        ((OutputFormat) outPutFormatInt) != OutputFormat.NONE)
                    {
                        outputFormat = (OutputFormat) outPutFormatInt;
                    }
                    else
                    {
                        throw new OptionException(
                            "unknown output format value (0 for MGF, 1 for mzMl, 2 for indexed mzML, 3 for Parquet, 4 for MGF with profile date excluded)",
                            "-f, --format");
                    }
                }

                if (outputMetadataString != null)
                {
                    int metadataInt;
                    try
                    {
                        metadataInt = int.Parse(outputMetadataString);
                    }
                    catch (FormatException e)
                    {
                        throw new OptionException("unknown metadata format value (0 for JSON, 1 for TXT)",
                            "-m, --metadata");
                    }

                    if (Enum.IsDefined(typeof(MetadataFormat), metadataInt) &&
                        ((MetadataFormat) metadataInt) != MetadataFormat.NONE)
                    {
                        outputMetadataFormat = (MetadataFormat) metadataInt;
                    }
                    else
                    {
                        throw new OptionException("unknown metadata format value (0 for JSON, 1 for TXT)",
                            "-m, --metadata");
                    }
                }
            }
            catch (OptionException optionException)
            {
                ShowHelp("Error - usage is (use -option=value for the optional arguments):", optionException,
                    optionSet);
            }
            catch (ArgumentNullException argumentNullException)
            {
                if (help)
                {
                    ShowHelp(" usage is (use -option=value for the optional arguments):", null,
                        optionSet);
                }
                else
                {
                    ShowHelp("Error - usage is (use -option=value for the optional arguments):", null,
                        optionSet);
                }
            }

            try
            {
                if (verbose)
                {
                    ((log4net.Repository.Hierarchy.Hierarchy) LogManager.GetRepository()).Root.Level =
                        Level.Debug;
                    ((log4net.Repository.Hierarchy.Hierarchy) LogManager.GetRepository())
                        .RaiseConfigurationChanged(EventArgs.Empty);
                }

                var parseInput = new ParseInput(rawFilePath, outputDirectory, outputFormat, gzip, outputMetadataFormat,
                    s3url, s3AccessKeyId, s3SecretAccessKey, bucketName, ignoreInstrumentErrors);
                RawFileParser.Parse(parseInput);
            }
            catch (Exception ex)
            {
                Log.Error("An unexpected error occured:");
                Log.Error(ex.ToString());

                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Show the help message
        /// </summary>
        /// <param name="message">the help message</param>
        /// <param name="optionException">the option exception, can be null</param>
        /// <param name="optionSet">the option set object</param>
        private static void ShowHelp(string message, OptionException optionException, OptionSet optionSet)
        {
            if (optionException != null)
            {
                if (!optionException.OptionName.IsNullOrEmpty())
                {
                    Console.Error.Write(optionException.OptionName + ": ");
                }

                Console.Error.WriteLine(optionException.Message);
            }

            Console.Error.WriteLine(message);
            optionSet.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }
    }
}