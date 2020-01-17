using Amazon;
using Amazon.CDK;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.CXAPI;
using System;
using System.IO;
using System.Linq;

namespace AwsStack
{
    class Program
    {
        static void Main(string[] args)
        {
            //Here I move the working directory to the directory with the solution file so all logic can be written based on root directory.
            while (Directory.GetFiles(Directory.GetCurrentDirectory()).Select(f => Path.GetFileName(f)).Contains("AwsCdkDockerSample.sln") == false)
            {
                Directory.SetCurrentDirectory(Directory.GetParent(Directory.GetCurrentDirectory()).ToString());
            }

            //Here I create a temp folder and copy all the project files into it.
            //without doing this the cdk.out folder gets recopied and nested in an infinate loop.
            DockerFolderBuilder.CreateBuildFolder();

            AppProps props = new AppProps()
            {
                Outdir = "cdk.out"
            };

            StackProps stackProps = new StackProps()
            {
                Env = new Amazon.CDK.Environment()
                {
                    Account = "DummyAccountId",
                    Region = RegionEndpoint.USEast1.SystemName
                }
            };

            var app = new App(props);

            var studioStack = new AppStack(app, "AppStack", stackProps);

            if (System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
            {
                var devStack = new DevStack(app, "DevStack", stackProps);
                studioStack.AddDependency(devStack);
            }

            CloudAssembly appAssembly = app.Synth();

            DockerFolderBuilder.CleanUpAllBuildFolders();

        }
    }

}
