using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.IAM;
using System.IO;

namespace AwsStack
{
    public class AppStack : Stack
    {
        public AppStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            var cluster = new Cluster(this, "AppCluster", new ClusterProps
            {
                Vpc = Vpc.FromLookup(this, "DefaultVpc", new VpcLookupOptions()
                {
                    IsDefault = true
                }),
            });

            new ApplicationLoadBalancedFargateService(this, "AppService",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = cluster,
                    DesiredCount = 1,
                    TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                    {
                        Image = ContainerImage.FromAsset(DockerFolderBuilder.GetBuildFolder(), new AssetImageProps()
                        {
                            File = Path.Combine("DockerProject1", "Dockerfile")
                        }),
                        TaskRole = new Role(this, "AppRole", new RoleProps()
                        {
                            RoleName = "AppRole",
                            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
                            ManagedPolicies = new IManagedPolicy[]
                            {
                                ManagedPolicy.FromAwsManagedPolicyName("TranslateFullAccess"),
                                ManagedPolicy.FromAwsManagedPolicyName("AmazonS3FullAccess"),
                            }
                        })
                    },
                    MemoryLimitMiB = 2048,      // Default is 256
                    PublicLoadBalancer = true,    // Default is false
                    AssignPublicIp = true,
                }
            );
        }
    }

}
