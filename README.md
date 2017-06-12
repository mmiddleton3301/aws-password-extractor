# AWS Password Extractor
[![Build status](https://ci.appveyor.com/api/projects/status/5iv6tqkvkm7b7xct?svg=true)](https://ci.appveyor.com/project/mmiddleton3301/aws-password-extractor)

A console application (and C# library) designed to pull back all AWS EC2 instances, their IP addresses and root/administrator passwords for a given access key.

## Installation
**aws-pe** does not need to be "installed" - just download the latest zip/build artifact from [AppVeyor](https://ci.appveyor.com/project/mmiddleton3301/aws-password-extractor/build/artifacts) and extract to a directory to use the executable.

### Coming Soon

- Zips available via GitHub releases (when I can be bothered to figure out how to push releases to GitHub from AppVeyor);
- Download via [chocolately](https://chocolatey.org/).

## Usage
Simply invoke `aws-pwe` to view the options available:

    --accesskeyid                  If not using a credentials file, the AWS access key ID.
    
    --secretaccesskey              If not using a credentials file, the secret access key.
    
    --awsregion                    Required. The AWS region in which your instances reside. For example, "eu-west-1".
    
    --outputfile                   Required. The output location to contain instance details. If a file exists already, then it will be overwritten.
     
    --passwordencryptionkeyfile    Required. The password encryption key file, used when originally spawning the EC2 instances. This is used in decrypting the password data pulled back from the API.
    
    --rolearn                      If assuming a particular role as part of your request, specify the role ARN with this option.
    
    --verbosity                    (Default: Warn) Specify the verbosity of output from the application. Valid options are: "Debug", "Info", "Warn", "Error", "Fatal" or "Off".
    
    --help                         Display this help screen.
    
    --version                      Display version information.
    

### `.credentials` file
It is recommended that you create a `.credentials` file in your Windows user profile. This contains your AWS Access Key and Secret Access Key. For more information how to create this file, [follow the instructions on setting up the AWS CLI](http://docs.aws.amazon.com/cli/latest/userguide/cli-chap-getting-started.html). This will means you wont have to explicitly state your `--accesskeyid` and `--secretaccesskey`.

The `--accesskeyid` and `--secretaccesskey` options always override the `.credentials` file - therefore, if you have a `.credentials` file and have specified these options, the options will always take priority in authenticating to your account.

### Role Support
You can assume a different AWS role by specifying the `--rolean`. Your specified IAM account must have access to the specified role ARN.
