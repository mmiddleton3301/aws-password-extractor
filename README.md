# AWS Password Extractor
[![Build status](https://ci.appveyor.com/api/projects/status/5iv6tqkvkm7b7xct?svg=true)](https://ci.appveyor.com/project/mmiddleton3301/aws-password-extractor) [![Downloads Badge](https://img.shields.io/chocolatey/dt/aws-password-extractor.svg)](https://chocolatey.org/packages/aws-password-extractor) [![Version Badge](https://img.shields.io/chocolatey/v/aws-password-extractor.svg)](https://chocolatey.org/packages/aws-password-extractor)

A console application (and C# library) designed to pull back all AWS EC2 instances, their IP addresses and root/administrator passwords for a given access key.

## Installation
### Chocolatey
The recommended way of installing **aws-pwe** is via [chocolatey](https://chocolatey.org/):

`choco install aws-password-extractor `

You can then simply invoke **aws-pwe** from the command line! As easy as that!

### Manually
**aws-pwe** can be downloaded and "installed" manually - just download the latest zip/build artifact from [AppVeyor](https://ci.appveyor.com/project/mmiddleton3301/aws-password-extractor/build/artifacts) and extract to a directory to use the executable.

### Coming Soon

- Zips available via GitHub releases (when I can be bothered to figure out how to push releases to GitHub from AppVeyor)

## Usage
Simply invoke `aws-pwe` to view the options available:

    --accesskeyid                           If not using a credentials file, the AWS access key ID.
    
    --secretaccesskey                       If not using a credentials file, the secret access key.
    
    --awsregion                             Required. The AWS region in which your instances reside. For example, "eu-west-1".
    
    --outputfile                            Required. The output location to contain instance details. If a file exists already, then it will be overwritten.
     
    --passwordencryptionkeyfile             The password encryption key file for the EC2 instances.
    
    --passwordencryptionkeyfiledirectory    A directory containing password encryption key files for EC2 instances. Useful when you have multiple key files for a single environment. All valid key files in this directory will be used to attempt decryption of EC2 passwords.  
    
    --rolearn                               If assuming a particular role as part of your request, specify the role ARN with this option.
    
    --verbosity                             (Default: Warn) Specify the verbosity of output from the application. Valid options are: "Debug", "Info", "Warn", "Error", "Fatal" or "Off".
    
    --help                                  Display this help screen.
    
    --version                               Display version information.
    

### `.credentials` file
It is recommended that you create a `.credentials` file in your Windows user profile. This contains your AWS Access Key and Secret Access Key. For more information how to create this file, [follow the instructions on setting up the AWS CLI](http://docs.aws.amazon.com/cli/latest/userguide/cli-chap-getting-started.html). This means you won't have to explicitly state your `--accesskeyid` and `--secretaccesskey`.

The `--accesskeyid` and `--secretaccesskey` options always override the `.credentials` file - therefore, if you have a `.credentials` file and have specified these options, the options will always take priority in authenticating to your account.

### Role Support
You can assume a different AWS role by specifying the `--rolean`. Your specified IAM account must have access to the specified role ARN.

### Support for multiple EC2 password encryption files
As of v1.1.0 you can now specify a directory containing multiple EC2 password encryption files using `--passwordencryptionkeyfiledirectory`. When pulling back instance information, `aws-pwe` will attempt to decrypt the instance password with each key located in that directory until it either gets it right, or runs out more keys to try.

Alternatively, if you just have one key, you can still invoke `--passwordencryptionkeyfile`.
