using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Web;
using MSharp.Framework.Services;

namespace MSharp.Framework.AWS
{
    // Note: In web.config, specify: <add key="UploadFolder.VirtualRoot" value="/s3?" />
    // Also in web.config under <system.webServer><handlers>, add: 
    // <add name = "S3 File handler" path="s3" verb="GET" type="MSharp.Framework.AWS.WebRequestHandler, MSharp.Framework.Web" preCondition="integratedMode" />

    public class WebRequestHandler : IHttpHandler
    {
        static ConcurrentDictionary<string, Type> RegisteredOwnerTypes = new ConcurrentDictionary<string, Type>();
        HttpResponse Response;
        string[] PathParts;
        Type Type;
        string Property;
        object Instance;
        Document Document;

        /// <summary>
        /// Call this in Global.asax for the entities model assembly.
        /// </summary>
        public static void RegisterDocumentOwnerTypesContainer(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t => t.Implements<IEntity>()))
                RegisteredOwnerTypes.TryAdd(type.Name, type);
        }

        IUser CurrentUser => UserServices.AuthenticationProvider.LoadUser(HttpContext.Current.User);

        public void ProcessRequest(HttpContext context)
        {
            Response = context.Response;
            PathParts = context.Request.QueryString[0].UrlDecode().TrimStart('/').Split('/').ToArray();
            FindRequestedProperty();
            FindRequestedObject();
            Dispatch();
        }

        void Dispatch()
        {
            if (Document.FileAccessMode == Document.AccessMode.Secure)
                EnsureSecurity();

            var data = Document.FileData;
            var name = Document.FileName;

            try
            {
                Response.Dispatch(data, name);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("The remote host closed the connection.")) return;
                else throw;
            }
        }

        void FindRequestedObject()
        {
            var id = PathParts.Last().Split('.').First();
            Instance = Database.Get(id, Type);

            Document = EntityManager.ReadProperty(Instance, Property) as Document;
        }

        void FindRequestedProperty()
        {
            var typeName = PathParts[0].Split('.')[0];
            var typeInfo = PathParts.ElementAt(PathParts.Count() - 2).Split('.');

            Type = RegisteredOwnerTypes.GetOrDefault(typeInfo.First());
            if (Type == null) throw new Exception("Invalid type name specified: '{0}'".FormatWith(typeName));

            Property = PathParts[0].Split('.')[1];
        }

        void EnsureSecurity()
        {
            try
            {
                var method = Type.GetMethod("Is" + Property + "VisibleTo", BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    throw new Exception(Type.FullName + ".Is" + Property + "VisibleTo() method is not defined.");
                }

                if (method.GetParameters().Count() != 1 || !method.GetParameters().Single().ParameterType.Implements<IUser>())
                    throw new Exception(Type.FullName + "." + method.Name + "() doesn't accept a single argument that implements IUser");

                if (!(bool)method.Invoke(Instance, new object[] { CurrentUser }))
                    throw new Exception("You are not authorised to view the requested file.");
            }
            catch (Exception ex)
            {
                Response.Clear();
                Response.Write("<html><body><h2>File access issue</h2></body></html>");
                Log.Error("Invalid secure file access: " + PathParts.ToString("/"), ex);

                Response.WriteLine("Invalid file request. Please contact your I.T. support.");
                Response.WriteLine(ex.Message);

                Response.End();
                return;
            }
        }

        public bool IsReusable => false;
    }
}