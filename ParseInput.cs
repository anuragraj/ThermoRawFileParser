﻿using System.IO;
using ThermoRawFileParser.Writer;

namespace ThermoRawFileParser
{
    public class ParseInput
    {
        /// <summary>
        /// The RAW file path.
        /// </summary>
        public string RawFilePath { get; }

        /// <summary>
        /// The output directory.
        /// </summary>
        public string OutputDirectory { get; }

        /// <summary>
        /// The output format.
        /// </summary>
        public OutputFormat OutputFormat { get; }

        /// <summary>
        /// Gzip the output file.
        /// </summary>
        public bool Gzip { get; }

        /// <summary>
        /// Output the metadata.
        /// </summary>
        public MetadataFormat OutputMetadata { get; }

        /// <summary>
        /// The raw file name.
        /// </summary>
        public string RawFileName { get; }

        /// <summary>
        /// The RAW file name without extension.
        /// </summary>
        public string RawFileNameWithoutExtension { get; }

        private S3Loader S3Loader { get; set; }

        private string S3AccessKeyId { get; }

        private string S3SecretAccessKey { get; }

        private string S3url { get; }

        public bool IgnoreInstrumentErrors { get; }

        private string bucketName;

        public ParseInput(string rawFilePath, string outputDirectory, OutputFormat outputFormat, bool gzip,
            MetadataFormat outputMetadata, string s3url, string s3AccessKeyId,
            string s3SecretAccessKey, string bucketName,
            bool ignoreInstrumentErrors
        )
        {
            RawFilePath = rawFilePath;
            var splittedPath = RawFilePath.Split('/');
            RawFileName = splittedPath[splittedPath.Length - 1];
            RawFileNameWithoutExtension = Path.GetFileNameWithoutExtension(RawFileName);
            OutputDirectory = outputDirectory;
            OutputFormat = outputFormat;
            Gzip = gzip;
            OutputMetadata = outputMetadata;
            S3url = s3url;
            S3AccessKeyId = s3AccessKeyId;
            S3SecretAccessKey = s3SecretAccessKey;
            this.bucketName = bucketName;
            IgnoreInstrumentErrors = ignoreInstrumentErrors;

            if (S3url != null && S3AccessKeyId != null && S3SecretAccessKey != null && bucketName != null)
                InitializeS3Bucket();
        }

        private void InitializeS3Bucket()
        {
            S3Loader = new S3Loader(S3url, S3AccessKeyId, S3SecretAccessKey, bucketName);
        }
    }
}