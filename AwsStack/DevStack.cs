using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using System.Collections.Generic;
using System.IO;

namespace AwsStack
{
    public class DevStack : Stack
    {
        public DevStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            var cluster = new Cluster(this, "AppCluster", new ClusterProps
            {
                Vpc = Vpc.FromLookup(this, "DefaultVpc", new VpcLookupOptions()
                {
                    IsDefault = true
                }),
            });

            FargateTaskDefinition taskDef = new FargateTaskDefinition(this, "DevStackTaskDef", new FargateTaskDefinitionProps()
            {
            });

            var mysqlContainer = taskDef.AddContainer("mysql", new ContainerDefinitionProps()
            {
                Image = ContainerImage.FromAsset(DockerFolderBuilder.GetBuildFolder(), new AssetImageProps()
                {
                    File = Path.Combine("LocalStack", "Dockerfile.mysql")
                })
            });
            mysqlContainer.AddPortMappings(new PortMapping()
            {
                HostPort = 3306,
                ContainerPort = 3306,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP
            });

            var mongoContainer = taskDef.AddContainer("mongodb", new ContainerDefinitionProps()
            {
                Image = ContainerImage.FromAsset(DockerFolderBuilder.GetBuildFolder(), new AssetImageProps()
                {
                    File = Path.Combine("LocalStack", "Dockerfile.mongo")
                })
            });
            mongoContainer.AddPortMappings(new PortMapping()
            {
                HostPort = 27017,
                ContainerPort = 27017,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP
            });

            var localStackContainer = taskDef.AddContainer("localstack", new ContainerDefinitionProps()
            {
                Image = ContainerImage.FromAsset(DockerFolderBuilder.GetBuildFolder(), new AssetImageProps()
                {
                    File = Path.Combine("LocalStack", "Dockerfile.localstack")
                })
            });
            localStackContainer.AddPortMappings(new PortMapping()
            {
                HostPort = 4572,
                ContainerPort = 4572,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP
            });
            localStackContainer.AddPortMappings(new PortMapping()
            {
                HostPort = 4576,
                ContainerPort = 4576,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP
            });
            localStackContainer.AddPortMappings(new PortMapping()
            {
                HostPort = 4584,
                ContainerPort = 4584,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP
            });

            var schemabuilder = taskDef.AddContainer("schemaBuilder", new ContainerDefinitionProps()
            {
                Image = ContainerImage.FromAsset(DockerFolderBuilder.GetBuildFolder(), new AssetImageProps()
                {
                    File = Path.Combine("LocalStack", "Dockerfile")
                })
            });

            schemabuilder.AddContainerDependencies(new ContainerDependency[] {
                new ContainerDependency()
                {
                    Container = mysqlContainer,
                    Condition = ContainerDependencyCondition.START
                },
                new ContainerDependency()
                {
                    Container = mongoContainer,
                    Condition = ContainerDependencyCondition.START
                },
                new ContainerDependency()
                {
                    Container = localStackContainer,
                    Condition = ContainerDependencyCondition.START
                }
            });

            // Create a load-balanced Fargate service and make it public
            new ApplicationLoadBalancedFargateService(this, "DevStackService",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = cluster,          // Required
                    DesiredCount = 1,
                    TaskDefinition = taskDef,
                    MemoryLimitMiB = 2048,      // Default is 256
                    PublicLoadBalancer = true,    // Default is false
                    AssignPublicIp = true
                }
            );


        }
    }

}
