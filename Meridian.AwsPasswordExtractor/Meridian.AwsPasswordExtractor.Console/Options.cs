// ----------------------------------------------------------------------------
// <copyright file="Options.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Console
{    
    using System.IO;
    using CommandLine;

    /// <summary>
    /// An options class, used by the <see cref="CommandLine.Parser" /> to
    /// provide option metadata and clean parsing.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets the AWS access key.
        /// If specified, then <see cref="Options.SecretAccessKey" /> also
        /// needs to be specified. 
        /// Takes precedence over any AWS credentials file that may be present
        /// in the local Windows user profile file system.
        /// </summary>
        [Option(
            HelpText = "If not using a credentials file, the AWS access key " +
                "ID.")]
        public string AccessKeyId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the AWS secret access key.
        /// If specified, then <see cref="Options.AccessKeyId" /> also
        /// needs to be specified.
        /// Takes precedence over any AWS credentials file that may be present
        /// in the local Windows user profile file system.
        /// </summary>
        [Option(
            HelpText = "If not using a credentials file, the secret access " +
                "key.")]
        public string SecretAccessKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the AWS region in which to execute AWS SDK methods
        /// against.
        /// </summary>
        [Option(
            Required = true,
            HelpText = "The AWS region in which your instances reside. For " +
                "example, \"eu-west-1\".")]
        public string AwsRegion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location of the output file.
        /// </summary>
        [Option(
            Required = true,
            HelpText = "The output location to contain instance details. If " +
                "a file exists already, then it will be overwritten.")]
        public FileInfo OutputFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location of the password encryption key file.
        /// </summary>
        [Option(
            Required = false,
            HelpText = "The password encryption key file for the EC2 " +
            "instances.")]
        public FileInfo PasswordEncryptionKeyFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a directory containing password encryption key files.
        /// </summary>
        [Option(
            Required = false,
            HelpText = "A directory containing password encryption key " +
            "files for EC2 instances. Useful when you have multiple key " +
            "files for a single environment. All valid key files in this " +
            "directory will be used to attempt decryption of EC2 passwords.")]
        public DirectoryInfo PasswordEncryptionKeyFileDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a role ARN to assume. Optional.
        /// </summary>
        [Option(
            HelpText = "If assuming a particular role as part of your " +
                "request, specify the role ARN with this option.")]
        public string RoleArn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a logging level to use. Optional - default value is
        /// <see cref="VerbosityOption.Warn" />. 
        /// </summary>
        [Option(
            HelpText = "Specify the verbosity of output from the " +
                "application. Valid options are: \"Debug\", \"Info\", " +
                "\"Warn\", \"Error\", \"Fatal\" or \"Off\".",
            Default = VerbosityOption.Warn)]
        public VerbosityOption Verbosity
        {
            get;
            set;
        }
    }
}