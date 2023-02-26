using Neo4jClient;
using Neo4jClient.Cypher;
using Org.BouncyCastle.Crypto;

namespace BackendGVK.Extensions
{
    public class GraphConstraints
    {
        private readonly ICypherFluentQuery _cypher;
        private const string constraintQuery = @"CONSTRAINT {0} IF NOT EXISTS
                                                 FOR ({1}:{2}) REQUIRE {1}.{3} IS UNIQUE";

        private List<string> constraintQueries = new List<string>();
        public GraphConstraints(ICypherFluentQuery cypher)
        {
            _cypher = cypher;
        }

        public GraphConstraints AddUniqueConstraint(string nodeType, string propertyName)
        {
            if (nodeType == null || propertyName == null) throw new ArgumentNullException();

            string constraintName = nodeType.ToLower() + "_" + propertyName.ToLower();
            string identity = nodeType.ToLower();

            string query = string.Format(constraintQuery, constraintName, identity, nodeType, propertyName);
            constraintQueries.Add(query);

            return this;
        }

        public async Task ExecuteAsync()
        {
            if (constraintQueries.Count == 0) throw new InvalidOperationException("The list of constraints is empty.");

            foreach (var constraint in constraintQueries)
            {
                await _cypher
                    .Create(constraint)
                    .ExecuteWithoutResultsAsync();
            }
        }
    }
}
