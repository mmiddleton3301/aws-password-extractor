// ----------------------------------------------------------------------------
// <copyright file="IOutputFileGenerator.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic.Definitions
{
    using System;
    using System.IO;

    /// <summary>
    /// Describes the operations of the output file generator.
    /// </summary>
    public interface IOutputFileGenerator
    {
        /// <summary>
        /// Creates an output file containing
        /// <see cref="Models.InstanceDetail" />. 
        /// </summary>
        /// <param name="awsAccessKeys">
        /// An instance of <see cref="Tuple{string, string}" /> containing
        /// firstly the access key id, followed by the the secret access key.
        /// Either value can be null.
        /// </param>
        /// <param name="region">
        /// The AWS region in which to execute AWS SDK methods against.
        /// </param>
        /// <param name="passwordEncryptionKeyFile">
        /// The location of the password encryption key file.
        /// </param>
        /// <param name="roleArn">
        /// An IAM role ARN to assume prior to pulling back EC2
        /// <see cref="InstanceDetail" />. Optional.
        /// </param>
        /// <param name="outputFile">
        /// The location of the output file.
        /// </param>
        /// <returns>
        /// Returns true if the process completed with success, otherwise
        /// false.
        /// </returns>
        bool CreateOutputFile(
            Tuple<string, string> awsAccessKeys,
            string region,
            FileInfo passwordEncryptionKeyFile,
            string roleArn,
            FileInfo outputFile);
    }
}