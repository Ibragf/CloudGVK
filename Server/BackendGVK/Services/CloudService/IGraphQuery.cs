using BackendGVK.Models;
using Neo4jClient;
using Neo4jClient.Cypher;
using System.Text;

namespace BackendGVK.Services.CloudService
{
    public class GraphQuery<T>
    {
        private readonly IGraphClient _client;
        public GraphQuery(ElementTypes type, IGraphClient client)
        {
            Type = type;
            _client = client;
        }

        private Dictionary<string, string> WhereValues = new Dictionary<string, string>();
        private Dictionary<string, string> UpdateValues = new Dictionary<string, string>();
        private readonly ElementTypes Type;


        public GraphQuery<T> Where(string propertyName, string value)
        {
            WhereValues.Add(propertyName, value);
            return this;
        }

        public GraphQuery<T> Update(string propertyName, string value)
        {
            UpdateValues.Add(propertyName, value);
            return this;
        }

        public async Task<IEnumerable<T>> ExecuteAsync()
        {
            string whereString = null!;
            string updateString;
            if (WhereValues.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                var keys = WhereValues.Keys;
                int count = 0;
                foreach (var key in keys)
                {
                    sb.Append(key).Append(":").Append("$").Append(key);
                    if (count < WhereValues.Count) sb.Append(",");
                    count++;
                }
                sb.Append("}");
                whereString = sb.ToString();

                string type = Type.ToString();
                ICypherFluentQuery cypher = _client.Cypher.OptionalMatch($"(e:{type} {whereString})");
                foreach (var key in WhereValues.Keys)
                {
                    cypher.WithParam(key, WhereValues[key]);
                }

                if (UpdateValues.Count > 0)
                {
                    sb.Clear();
                    foreach (var key in UpdateValues.Keys)
                    {
                        sb.Append("e.").Append(key).Append("=").Append("$").Append(key);
                        cypher.WithParam(key, UpdateValues[key]);
                        UpdateValues.Remove(key);
                        if (UpdateValues.Count > 0) sb.Append(",");
                    }
                    updateString = sb.ToString();

                    cypher.Set($"{updateString}");
                }

                var result = await cypher.Return(e => e.As<T>()).ResultsAsync;

                return result;
            }

            return null!;
        }
    }
}
