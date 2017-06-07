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
        /// Initialises a new instance of the <see cref="InstanceScanner" />
        /// class.
        /// </summary>
        /// <param name="fileSystemProvider">
        /// An instance of <see cref="IFileSystemProvider" />. 
        /// </param>
        public InstanceScanner(IFileSystemProvider fileSystemProvider)
        {
            // TODO: Inject somehow the AWS instances - as the AWS SDK seems to
            //       be losely coupled.
            this.fileSystemProvider = fileSystemProvider;
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
            string roleArn)
        {
            InstanceDetail[] toReturn = null;

            // If an access key and secret key were passed in via the command
            // line, use these details as a matter of priority.
            AWSCredentials explicitCredentials = null;
            if (awsAccessKeys != null)
            {
                explicitCredentials = new BasicAWSCredentials(
                    awsAccessKeys.Item1,
                    awsAccessKeys.Item2);
            }

            RegionEndpoint regionEndpoint =
                RegionEndpoint.GetBySystemName(region);

            AmazonEC2Config amazonEC2Config = new AmazonEC2Config()
            {
                RegionEndpoint = regionEndpoint
            };

            IAmazonEC2 amazonEC2 = null;
            if (string.IsNullOrEmpty(roleArn))
            {
                if (explicitCredentials == null)
                {
                    amazonEC2 = new AmazonEC2Client(amazonEC2Config);
                }
                else
                {
                    amazonEC2 = new AmazonEC2Client(
                        explicitCredentials,
                        amazonEC2Config);
                }
            }
            else
            {
                amazonEC2 = this.AssumeRoleAndCreateEC2Client(
                    explicitCredentials,
                    amazonEC2Config,
                    roleArn);
            }

            Reservation[] allReservations =
                this.GetRegionReservations(amazonEC2);

            Instance[] allInstances = allReservations
                .SelectMany(x => x.Instances)
                .ToArray();

            // Read the password encryption key.
            string passwordEncryptionKey =
                this.fileSystemProvider.ReadFileInfoAsString(
                    passwordEncryptionKeyFile);

            toReturn = allInstances
                .Select(x => this.ConvertInstanceToInstanceDetail(
                    amazonEC2,
                    passwordEncryptionKey,
                    x))
                .ToArray();

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
                // Nope. Use the credentials file.
                amazonSecurityTokenService =
                    new AmazonSecurityTokenServiceClient(
                        amazonSecurityTokenServiceConfig);
            }
            else
            {
                // Yep.
                amazonSecurityTokenService =
                    new AmazonSecurityTokenServiceClient(
                        explicitCredentials,
                        amazonSecurityTokenServiceConfig);
            }

            // Just use the latter part of the ARN as the session name.
            string roleSessionName = roleArn.Split('/')
                .Last();

            AssumeRoleRequest assumeRoleRequest = new AssumeRoleRequest()
            {
                RoleArn = roleArn,
                RoleSessionName = roleSessionName
            };

            // Get the temporary credentials using the specified role.
            AssumeRoleResponse assumeRoleResponse =
                amazonSecurityTokenService.AssumeRole(assumeRoleRequest);

            Credentials roleCreds = assumeRoleResponse.Credentials;

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
        /// <param name="passwordEncryptionKey">
        /// The AWS password encryption key, used in decryption.
        /// </param>
        /// <param name="instance">
        /// An instance of <see cref="Instance" />. 
        /// </param>
        /// <returns>
        /// An instance of <see cref="InstanceDetail" />. 
        /// </returns>
        private InstanceDetail ConvertInstanceToInstanceDetail(
            IAmazonEC2 amazonEC2,
            string passwordEncryptionKey,
            Instance instance)
        {
            InstanceDetail toReturn = new InstanceDetail()
            {
                IPAddress = IPAddress.Parse(instance.PrivateIpAddress)
            };

            // TODO: Investigate whether names are always stored in the tags?
            Tag nameTag = instance.Tags.Single(x => x.Key == "Name");

            toReturn.Name = nameTag.Value;

            string instancePassword = this.GetInstancePassword(
                amazonEC2,
                passwordEncryptionKey,
                instance.InstanceId);

            toReturn.Password = instancePassword;

            return toReturn;
        }

        /// <summary>
        /// Gets a particular instance's password (or at least, the original
        /// password assigned to the EC2 instance upon creation).
        /// </summary>
        /// <param name="amazonEC2">
        /// An instance of <see cref="IAmazonEC2" />. 
        /// </param>
        /// <param name="passwordEncryptionKey">
        /// The AWS password encryption key, used in decryption.
        /// </param>
        /// <param name="instanceId">
        /// The id of the <see cref="Instance" /> to pull back a password for.
        /// </param>
        /// <returns>
        /// The instance's password, as a <see cref="string" /> .
        /// </returns>
        private string GetInstancePassword(
            IAmazonEC2 amazonEC2,
            string passwordEncryptionKey,
            string instanceId)
        {
            string toReturn = null;

            GetPasswordDataRequest getPasswordDataRequest =
                new GetPasswordDataRequest(instanceId);

            GetPasswordDataResponse getPasswordDataResponse =
                amazonEC2.GetPasswordData(getPasswordDataRequest);

            try
            {
                toReturn = getPasswordDataResponse
                    .GetDecryptedPassword(passwordEncryptionKey);
            }
            catch (CryptographicException)
            {
                // We still want to carry on if we can't decrypt the password,
                // but...
                // TODO: Log the fact that the decryption key seems to be
                //       incorrect!
            }

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
            do
            {
                describeInstancesResponse =
                    amazonEC2.DescribeInstances(describeInstancesRequest);

                allReservations.AddRange(
                    describeInstancesResponse.Reservations);

                describeInstancesRequest.NextToken =
                    describeInstancesResponse.NextToken;
            }
            while (describeInstancesResponse.NextToken != null);

            toReturn = allReservations.ToArray();

            return toReturn;
        }
    }
}