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
            Required = true,
            HelpText = "The password encryption key file, used when " +
            "originally spawning the EC2 instances. This is used in " +
            "decrypting the password data pulled back from the API.")]
        public FileInfo PasswordEncryptionKeyFile
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
    }
}