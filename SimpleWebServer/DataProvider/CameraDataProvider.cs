using SimpleWebServer.model;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebServer.DataProvider
{
    public sealed class CameraDataProvider
    {

        SQLite.Net.SQLiteConnection conn;
        String path;

        public CameraDataProvider()
        {
            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");

            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            conn.CreateTable<Camera>();
        }

        public Camera CreateCamera(Camera cameraToCreate)
        {
            conn.Insert(cameraToCreate);
            return cameraToCreate;
        }

        public Camera ReadCamera(Guid id)
        {
            Camera result = null;

            SQLiteCommand cmd = conn.CreateCommand("select * from camera where id=?;", id);
            List<Camera> results = cmd.ExecuteQuery<Camera>();

            if (results.Count > 0)
            {
                result = results[0];
            }

            return result;
        }

        public Camera UpdateCamera(Camera cameraToUpdate)
        {
            // First make sure that the ID represents an existing camera
            if ( ReadCamera(cameraToUpdate.id) == null )
            {
                return null;
            }

            conn.Update(cameraToUpdate);

            return cameraToUpdate;
        }

        public bool DeleteCamera(String id)
        {
            try
            {
                Guid gid = Guid.Parse(id);
                Camera camera = ReadCamera(gid);
                if (camera != null)
                {
                    conn.Delete(camera);
                    return true;
                }
            }
            catch ( FormatException )
            {
                
            }

            return false;
        }

        public IEnumerable<Camera> ListCameras( int start, int count)
        {

            SQLiteCommand cmd = conn.CreateCommand("select * from camera LIMIT ?, ?", start, count);
            IEnumerable<Camera> results = cmd.ExecuteQuery<Camera>();

            int resultCount = 0;
            using (IEnumerator<Camera> enumerator = results.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    resultCount++;
            }

            if (resultCount > 0)
            {
                return results;
            }

            return null;
        }
    }
}
