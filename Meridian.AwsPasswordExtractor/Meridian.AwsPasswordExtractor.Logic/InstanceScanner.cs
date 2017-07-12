// ----------------------------------------------------------------------------
// <copyright file="InstanceScanner.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using Amazon;
    using Amazon.EC2;
    using Amazon.EC2.Model;
    using Amazon.Runtime;
    using Amazon.SecurityToken;
    using Amazon.SecurityToken.Model;
    using Meridian.AwsPasswordExtractor.Logic.Definitions;
    using Meridian.AwsPasswordExtractor.Logic.Models;

    /// <summary>
    /// Implements <see cref="IInstanceScanner" />. 
    /// </summary>
    public class InstanceScanner : IInstanceScanner
    {
        /// <summary>
        /// The number of instances to return in one go - this value is
        /// basically fed into
        /// <see cref="DescribeInstancesRequest.MaxResults" />. 
        /// </summary>
        private const int DescribeInstancesMaxResults = 10;

        /// <summary>
        /// An instance of <see cref="IFileSystemProvider" />. 
        /// </summary>
        private readonly IFileSystemProvider fileSystemProvider;

        /// <summary>
        /// An instance of <see cref="ILoggingProvider" />. 
        /// </summary>
        private readonly ILoggingProvider loggingProvider;

        /// <summary>
        /// Initialises a new instance of the <see cref="InstanceScanner" />
        /// class.
        /// </summary>
        /// <param name="fileSystemProvider">
        /// An instance of <see cref="IFileSystemProvider" />. 
        /// </param>
        /// <param name="loggingProvider">
        /// An instance of <see cref="ILoggingProvider" />. 
        /// </param>
        public InstanceScanner(
            IFileSystemProvider fileSystemProvider,
            ILoggingProvider loggingProvider)
        {
            this.fileSystemProvider = fileSystemProvider;
            this.loggingProvider = loggingProvider;
        }

        /// <summary>
        /// Implements
        /// <see cref="IInstanceScanner.ExtractDetails(Tuple{string, string}, string, FileInfo, string)" />. 
        /// </summary>
        /// <param name="awsAccessKeys">
        /// An instance of <see cref="Tuple{string, string}" /> containing
        /// firstly the access key id, followed by the the secret access key.
        /// If the variable is null, then credentials based authentication
        /// needs to be used.
        /// </param>
        /// <param name="region">
        /// The AWS region in which to execute AWS SDK methods against.
        /// </param>
        /// <param name="passwordEncryptionKeyFile">
        /// The location of the password encryption key file.
        /// </param>
        /// <param name="passwordEncryptionKeyFileDir">
        /// The location of a directory containing multiple password encryption
        /// key files.
        /// </param>
        /// <param name="roleArn">
        /// An IAM role ARN to assume prior to pulling back EC2
        /// <see cref="InstanceDetail" />. Optional.
        /// </param>
        /// <returns>
        /// An array of <see cref="InstanceDetail" /> instances. 
        /// </returns>
        public InstanceDetail[] ExtractDetails(
            Tuple<string, string> awsAccessKeys,
            string region,
            FileInfo passwordEncryptionKeyFile,
            DirectoryInfo passwordEncryptionKeyFileDir,
            string roleArn)
        {
            InstanceDetail[] toReturn = null;

            // If an access key and secret key were passed in via the command
            // line, use these details as a matter of priority.
            AWSCredentials explicitCredentials = null;
            if (awsAccessKeys != null)
            {
                this.loggingProvider.Info(
                    $"Explicit credentials provided. Access key ID: " +
                    $"\"{awsAccessKeys.Item1}\".");

                explicitCredentials = new BasicAWSCredentials(
                    awsAccessKeys.Item1,
                    awsAccessKeys.Item2);
            }
            else
            {
                this.loggingProvider.Info(
                    $"No explicit credentials provided. Instead, the " +
                    $"credentials file will be used.");
            }

            this.loggingProvider.Debug(
                $"Parsing the provided {nameof(RegionEndpoint)}...");

            RegionEndpoint regionEndpoint =
                RegionEndpoint.GetBySystemName(region);

            if (regionEndpoint.DisplayName == "Unknown")
            {
                throw new InvalidDataException(
                    $"The specified AWS region is unknown. Please specify a " +
                    $"valid region, for example: " +
                    $"\"{RegionEndpoint.EUWest2.SystemName}\".");
            }

            this.loggingProvider.Info(
                $"{nameof(RegionEndpoint)} parsed: " +
                $"{regionEndpoint.DisplayName}.");

            AmazonEC2Config amazonEC2Config = new AmazonEC2Config()
            {
                RegionEndpoint = regionEndpoint
            };

            IAmazonEC2 amazonEC2 = null;
            if (string.IsNullOrEmpty(roleArn))
            {
                this.loggingProvider.Debug(
                    "No role ARN was provided. Therefore, the API will be " +
                    "called with credentials either provided explicitly, or " +
                    "stored in the credentials file.");

                if (explicitCredentials == null)
                {
                    this.loggingProvider.Debug(
                        $"No explicit credentials were provided. A " +
                        $"{nameof(AmazonEC2Client)} will be generated using " +
                        $"the credentials in the credentials file...");

                    amazonEC2 = new AmazonEC2Client(amazonEC2Config);
                }
                else
                {
                    this.loggingProvider.Debug(
                        $"Explicit credentials were provided. The " +
                        $"{nameof(AmazonEC2Client)} will be generated using " +
                        $"these explicit credentials.");

                    amazonEC2 = new AmazonEC2Client(
                        explicitCredentials,
                        amazonEC2Config);
                }
            }
            else
            {
                this.loggingProvider.Debug(
                    $"Role ARN provided: \"{roleArn}\". Credentials will be " +
                    $"requested for this role in order to create the " +
                    $"{nameof(AmazonEC2Client)}.");

                amazonEC2 = this.AssumeRoleAndCreateEC2Client(
                    explicitCredentials,
                    amazonEC2Config,
                    roleArn);
            }

            this.loggingProvider.Info($"{nameof(AmazonEC2Client)} created.");

            this.loggingProvider.Debug(
                $"Pulling back all available {nameof(Reservation)} " +
                $"instances...");

            Reservation[] allReservations =
                this.GetRegionReservations(amazonEC2);

            this.loggingProvider.Info(
                $"{allReservations} {nameof(Reservation)}(s) returned. " +
                $"Selecting out all {nameof(Instance)}(s)...");

            Instance[] allInstances = allReservations
                .SelectMany(x => x.Instances)
                .ToArray();

            this.loggingProvider.Info(
                $"{allInstances.Length} {nameof(Instance)}(s) selected.");

            FileInfo[] passwordEncryptionKeyFiles = null;

            if (passwordEncryptionKeyFile != null)
            {
                this.loggingProvider.Info(
                    $"A single {nameof(passwordEncryptionKeyFile)} was " +
                    $"specified, rather than a directory. " +
                    $"\"{passwordEncryptionKeyFile.FullName}\" will be used.");

                // Then just use the single password encryption key file
                // specified.
                passwordEncryptionKeyFiles = new FileInfo[]
                {
                    passwordEncryptionKeyFile
                };
            }
            else
            {
                // Then use the password encryption key files in the specified
                // directory.
                passwordEncryptionKeyFiles =
                    passwordEncryptionKeyFileDir.GetFiles();

                this.loggingProvider.Info(
                    $"{nameof(passwordEncryptionKeyFileDir)} was specified, " +
                    $"and therefore all {passwordEncryptionKeyFiles.Length} " +
                    $"file(s) in " +
                    $"\"{passwordEncryptionKeyFileDir.FullName}\" will be " +
                    $"read into memory as potential key files.");
            }

            string[] passwordEncryptionKeys =
                this.GetPasswordEncryptionKeysFromFiles(
                    passwordEncryptionKeyFiles);

            this.loggingProvider.Debug(
                $"Fetching passwords for all {allInstances.Length} " +
                $"{nameof(Instance)}(s)...");

            toReturn = allInstances
                .Select(x => this.ConvertInstanceToInstanceDetail(
                    amazonEC2,
                    passwordEncryptionKeys,
                    x))
                .ToArray();

            this.loggingProvider.Info(
                $"Password extraction complete for {toReturn.Length} " +
                $"{nameof(Instance)}(s).");

            return toReturn;
        }

        /// <summary>
        /// Constructs an <see cref="AmazonEC2Client" /> instance with the
        /// temporary details generated from role assumption.
        /// </summary>
        /// <param name="explicitCredentials">
        /// An instance of <see cref="AWSCredentials" /> containing explicitly
        /// declared credentials (i.e. from the command line). Can be null.
        /// </param>
        /// <param name="amazonEC2Config">
        /// An instance of <see cref="AmazonEC2Config" />. 
        /// </param>
        /// <param name="roleArn">
        /// An IAM role ARN to assume.
        /// </param>
        /// <returns>
        /// A configured instance of <see cref="AmazonEC2Client" />. 
        /// </returns>
        private AmazonEC2Client AssumeRoleAndCreateEC2Client(
            AWSCredentials explicitCredentials,
            AmazonEC2Config amazonEC2Config,
            string roleArn)
        {
            AmazonEC2Client toReturn = null;

            AmazonSecurityTokenServiceConfig amazonSecurityTokenServiceConfig =
                new AmazonSecurityTokenServiceConfig()
                {
                    // Nothing for now...
                };

            IAmazonSecurityTokenService amazonSecurityTokenService = null;

            // Explcit credentials supplied?
            if (explicitCredentials == null)
            {
                this.loggingProvider.Debug(
                    $"No explicit credentials provided. Creating an " +
                    $"instance of the " +
                    $"{nameof(AmazonSecurityTokenServiceClient)} using " +
                    "credentials stored in the credentials file...");

                // Nope. Use the credentials file.
                amazonSecurityTokenService =
                    new AmazonSecurityTokenServiceClient(
                        amazonSecurityTokenServiceConfig);
            }
            else
            {
                this.loggingProvider.Debug(
                    $"Explicit credentials provided. Creating an instance " +
                    $"of the {nameof(AmazonSecurityTokenServiceClient)} " +
                    $"using these details...");

                // Yep.
                amazonSecurityTokenService =
                    new AmazonSecurityTokenServiceClient(
                        explicitCredentials,
                        amazonSecurityTokenServiceConfig);
            }

            this.loggingProvider.Info(
                $"Instance of {nameof(AmazonSecurityTokenServiceClient)} " +
                $"established.");

            this.loggingProvider.Debug(
                $"Parsing role ARN \"{roleArn}\" to create the session " +
                $"name...");

            // Just use the latter part of the ARN as the session name.
            string roleSessionName = roleArn.Split('/')
                .Last();

            this.loggingProvider.Info(
                $"Session name created from ARN: \"{roleSessionName}\".");

            AssumeRoleRequest assumeRoleRequest = new AssumeRoleRequest()
            {
                RoleArn = roleArn,
                RoleSessionName = roleSessionName
            };

            this.loggingProvider.Debug(
                $"Pulling back credentials from " +
                $"{nameof(AmazonSecurityTokenServiceClient)} by assuming " +
                $"specified role...");

            // Get the temporary credentials using the specified role.
            AssumeRoleResponse assumeRoleResponse =
                amazonSecurityTokenService.AssumeRole(assumeRoleRequest);

            Credentials roleCreds = assumeRoleResponse.Credentials;

            this.loggingProvider.Info(
                $"Credentials returned. Access ID: " +
                $"\"{roleCreds.AccessKeyId}\". Returning " +
                $"an instance of {nameof(AmazonEC2Client)}.");

            toReturn = new AmazonEC2Client(
                roleCreds,
                amazonEC2Config);

            return toReturn;
        }

        /// <summary>
        /// Converts an instance of <see cref="Instance" /> to
        /// <see cref="InstanceDetail" />, so that the information can be used
        /// in constructing a list of whatever format.  
        /// </summary>
        /// <param name="amazonEC2">
        /// An instance of <see cref="IAmazonEC2" />. 
        /// </param>
        /// <param name="passwordEncryptionKeys">
        /// Multiple EC2 password encryption keys.
        /// </param>
        /// <param name="instance">
        /// An instance of <see cref="Instance" />. 
        /// </param>
        /// <returns>
        /// An instance of <see cref="InstanceDetail" />. 
        /// </returns>
        private InstanceDetail ConvertInstanceToInstanceDetail(
            IAmazonEC2 amazonEC2,
            string[] passwordEncryptionKeys,
            Instance instance)
        {
            InstanceDetail toReturn = null;

            this.loggingProvider.Debug(
                $"Constructing {nameof(InstanceDetail)} instance from EC2 " +
                $"{nameof(Instance)}...");

            toReturn = new InstanceDetail()
            {
                IPAddress = IPAddress.Parse(instance.PrivateIpAddress)
            };

            // TODO: Investigate whether names are always stored in the tags?
            Tag nameTag = instance.Tags.Single(x => x.Key == "Name");

            toReturn.Name = nameTag.Value;

            string instancePassword = this.GetInstancePassword(
                amazonEC2,
                passwordEncryptionKeys,
                instance.InstanceId);

            toReturn.Password = instancePassword;

            this.loggingProvider.Info(
                $"{nameof(InstanceDetail)} constructed: {toReturn}.");

            return toReturn;
        }

        /// <summary>
        /// Gets a particular instance's password (or at least, the original
        /// password assigned to the EC2 instance upon creation).
        /// </summary>
        /// <param name="amazonEC2">
        /// An instance of <see cref="IAmazonEC2" />. 
        /// </param>
        /// <param name="passwordEncryptionKeys">
        /// Multiple EC2 password encryption keys.
        /// </param>
        /// <param name="instanceId">
        /// The id of the <see cref="Instance" /> to pull back a password for.
        /// </param>
        /// <returns>
        /// The instance's password, as a <see cref="string" /> .
        /// </returns>
        private string GetInstancePassword(
            IAmazonEC2 amazonEC2,
            string[] passwordEncryptionKeys,
            string instanceId)
        {
            string toReturn = null;

            this.loggingProvider.Debug(
                $"Requesitng encrypted password data from " +
                $"{nameof(Instance)} ID \"{instanceId}\"...");

            GetPasswordDataRequest getPasswordDataRequest =
                new GetPasswordDataRequest(instanceId);

            GetPasswordDataResponse getPasswordDataResponse =
                amazonEC2.GetPasswordData(getPasswordDataRequest);

            this.loggingProvider.Info(
                $"Password data returned for {nameof(Instance)} ID " +
                $"\"{instanceId}\". Attempting to decrypt password with " +
                $"all {passwordEncryptionKeys.Length} password encryption " +
                $"key(s) in memory...");

            // Only keep going for as long as we don't have a key decrypted.
            string passwordEncryptionKey = null;
            for (int i = 0; i < passwordEncryptionKeys.Length && string.IsNullOrEmpty(toReturn); i++)
            {
                try
                {
                    passwordEncryptionKey = passwordEncryptionKeys[i];

                    this.loggingProvider.Debug(
                        $"Using password encryption key " +
                        $"{i + 1}/{passwordEncryptionKeys.Length} to " +
                        $"decrypt password for {nameof(Instance)} ID " +
                        $"\"{instanceId}\"...");

                    toReturn = getPasswordDataResponse
                        .GetDecryptedPassword(passwordEncryptionKey);

                    this.loggingProvider.Info(
                        $"Password for {nameof(Instance)} ID " +
                        $"\"{instanceId}\" decrypted with success.");
                }
                catch (CryptographicException)
                {
                    // We still want to carry on if we can't decrypt the
                    // password, but...
                    this.loggingProvider.Warn(
                        $"Could not decrypt password for {nameof(Instance)} " +
                        $"ID \"{instanceId}\" using password encryption key " +
                        $"{i + 1}/{passwordEncryptionKeys.Length}.");
                }
            }

            if (string.IsNullOrEmpty(toReturn))
            {
                this.loggingProvider.Error(
                    $"Could not decrypt password for {nameof(Instance)}: " +
                    $"{passwordEncryptionKeys.Length} password encryption " +
                    $"keys were tried, and none succeeded. Are you sure " +
                    $"that the key file(s) are correct?");
            }

            return toReturn;
        }

        /// <summary>
        /// Reads an array of <see cref="FileInfo" /> instances and returns the
        /// content of the files as an array of <see cref="string" /> values. 
        /// </summary>
        /// <param name="passwordEncryptionKeyFiles">
        /// An array of <see cref="FileInfo" /> instances, describing where the
        /// password encryption key files are.
        /// </param>
        /// <returns>
        /// The content of the files, as an array of <see cref="string" />
        /// values.
        /// </returns>
        private string[] GetPasswordEncryptionKeysFromFiles(
            FileInfo[] passwordEncryptionKeyFiles)
        {
            string[] toReturn = null;

            this.loggingProvider.Debug(
                $"Reading {passwordEncryptionKeyFiles.Length} key file(s) " +
                $"into memory...");

            toReturn = passwordEncryptionKeyFiles
                .Select(x =>
                {
                    this.loggingProvider.Debug(
                        $"Reading the password encryption key into memory " +
                        $"(located at " +
                        $"\"{x.FullName}\")...");

                    // Read the password encryption key.
                    string passwordEncryptionKey =
                        this.fileSystemProvider.ReadFileInfoAsString(x);

                    this.loggingProvider.Info(
                        $"Password encryption key read into memory - " +
                        $"length: {x.Length}.");

                    return passwordEncryptionKey;
                })
                .ToArray();

            return toReturn;
        }

        /// <summary>
        /// Fetches all of the <see cref="Reservation" />s from the configured 
        /// </summary>
        /// <param name="amazonEC2">
        /// An instance of <see cref="IAmazonEC2" /> to use in communicating
        /// with the AWS API.
        /// </param>
        /// <returns>
        /// An array of <see cref="Reservation" /> instances.
        /// </returns>
        private Reservation[] GetRegionReservations(IAmazonEC2 amazonEC2)
        {
            Reservation[] toReturn = null;

            // First, list the instances for the configured account.
            DescribeInstancesRequest describeInstancesRequest =
                new DescribeInstancesRequest()
                {
                    MaxResults = DescribeInstancesMaxResults
                };

            List<Reservation> allReservations = new List<Reservation>();
            DescribeInstancesResponse describeInstancesResponse = null;

            this.loggingProvider.Debug(
                $"Pulling back {nameof(Reservation)}(s) in blocks of " +
                $"{DescribeInstancesMaxResults}.");

            string nextTokenStr = null;
            do
            {
                this.loggingProvider.Debug(
                    $"Executing {nameof(AmazonEC2Client)}." +
                    $"{nameof(amazonEC2.DescribeInstances)}...");

                describeInstancesResponse =
                    amazonEC2.DescribeInstances(describeInstancesRequest);

                this.loggingProvider.Info(
                    $"{describeInstancesResponse.Reservations.Count()} " +
                    $"{nameof(Reservation)}(s) returned.");

                allReservations.AddRange(
                    describeInstancesResponse.Reservations);

                nextTokenStr =
                    string.IsNullOrEmpty(describeInstancesResponse.NextToken)
                            ?
                        "null! Dropping out of loop."
                            :
                        $"{describeInstancesResponse.NextToken}.";

                this.loggingProvider.Info(
                    $"Next token is: {nextTokenStr}");

                describeInstancesRequest.NextToken =
                    describeInstancesResponse.NextToken;
            }
            while (describeInstancesResponse.NextToken != null);

            toReturn = allReservations.ToArray();

            this.loggingProvider.Info(
                $"Returning {toReturn.Length} {nameof(Reservation)} " +
                $"instance(s).");

            return toReturn;
        }
    }
}