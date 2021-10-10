using Newtonsoft.Json;
using Scanner.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner
{
    class WorkQueue
    {
/*
        create table Workitems(ID uniqueidentifier,Data varchar(max),Created datetime)
        create index IX_Workitems_Created on Workitems (Created asc)
        create index IX_Workitems_ID on Workitems (ID asc)
*/

        public static WorkQueue Instance = new WorkQueue();

        public WorkItem Pop()
        {
            WorkItem result=null;
            Guid id=Guid.Empty;

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["SQLConnectionString"]))
            {
                SqlCommand command = new SqlCommand("select top 1 ID,Data from Workitems order by Created desc", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        result = JsonConvert.DeserializeObject<WorkItem>((string)reader["Data"]);
                        id = (Guid)reader["ID"];
                    }
                }
                finally
                {
                    reader.Close();
                }
                SqlCommand command2 = new SqlCommand("delete from Workitems where id=@id", connection);
                command2.Parameters.AddWithValue("id", id);
                command2.ExecuteNonQuery();
            }

            return result;
        }

        public void Push(WorkItem item)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["SQLConnectionString"]))
            {
                
                SqlCommand command = new SqlCommand("insert into Workitems(ID,Data,Created) values (@ID,@Data,GETUTCDATE())", connection);
                connection.Open();
                command.Parameters.AddWithValue("id", item.Id);
                command.Parameters.AddWithValue("Data", JsonConvert.SerializeObject(item));
                command.ExecuteNonQuery();
            }
        }
    }
}
