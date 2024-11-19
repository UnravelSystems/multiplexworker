using System.Text.Json.Serialization;
using S3RabbitMongo.MassTransit;

namespace S3RabbitMongo.Models;

public class StringTreeNode
{
    [JsonPropertyName("value")] 
    public String Value { get; set; }
    [JsonPropertyName("children")] 
    public List<StringTreeNode>? Children { get; set; }

    public StringTreeNode(string value)
    {
        Value = value;
    }

    public void AddChild(StringTreeNode child)
    {
        if (Children == null)
        {
            Children = new List<StringTreeNode>();
        }
        Children.Add(child);
    }
}