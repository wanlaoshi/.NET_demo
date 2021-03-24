using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using System.Text;
using System.Collections.Generic;
using System.Collections;


namespace Jointown.AutoUpdate.AutoUpdateLib
{
	/// <summary>
	/// Common 的摘要说明。
	/// </summary>
	public class Common
	{
		public Common()
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
		}
        
		public static readonly String LAST_ISSUE_TIME = "*LAST_ISSUE_DATETIME"; //必须*号开头
        public static readonly String LAST_VER = "*LAST_ISSUE_VER"; //必须*号开头
        public static readonly String ISSUE_FOLDER = "ISSUEFOLDER";

		public static readonly String AUTOUPDATE_CONFIG = "autoUpdateConfig.dat";

		public static readonly String EXTENSION = ".bzip2";

		public static readonly String FILELIST = "fileList.config";


		public void SerializerFile(object ob , String fileName)
		{
			System.IO.FileStream writer = null;
			try
			{
				System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			
				writer =  new System.IO.FileStream(fileName,System.IO.FileMode.OpenOrCreate , System.IO.FileAccess.Write);
				
				formatter.Serialize(writer, ob);

			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				writer.Close();
			}
		}


        /// <summary>
        /// 按二进制格式序列化对象
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <returns></returns>
        public byte[] Serialize(Object obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            IFormatter bf = new BinaryFormatter();
            bf.Serialize(memoryStream, obj);
            memoryStream.Position = 0;
            byte[] content= new byte[((int)memoryStream.Length) + 1];
            memoryStream.Read(content, 0, content.Length);
            return content;


        }

		public object Deserialize(String fileName , Type ob) 
		{
            if (!File.Exists(fileName)) return null;
			object instance = ob.Assembly.CreateInstance(ob.FullName);

			// Open the file containing the data that you want to deserialize.
			FileStream fs = new FileStream(fileName, System.IO.FileMode.Open , System.IO.FileAccess.Read);
			try 
			{
				BinaryFormatter formatter = new BinaryFormatter();

				// Deserialize the hashtable from the file and 
				// assign the reference to the local variable.
				instance = formatter.Deserialize(fs);
				return instance;
			}
			catch (SerializationException e) 
			{
				throw;
			}
			finally 
			{
				fs.Close();
			}
		}

		public object Deserialize(byte[] buffer , Type ob)
		{
            if (buffer == null) return null;
			System.IO.MemoryStream stream = new MemoryStream(buffer);

			object instance = ob.Assembly.CreateInstance(ob.FullName);
			try 
			{
				BinaryFormatter formatter = new BinaryFormatter();

				// Deserialize the hashtable from the file and 
				// assign the reference to the local variable.
				instance = formatter.Deserialize(stream);
				return instance;
			}
			catch (SerializationException e) 
			{
				throw;
			}
			finally 
			{
				stream.Close();
			}
		}

        public String getConfigFile(string path)
        {

            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,path+"."+AUTOUPDATE_CONFIG);
        }

        public Hashtable getServerHashtable(string wslParam, string clientVer,string clientPath)
        {
            Object obj = this.Deserialize(ServiceAgent.Instance(wslParam).getUpdateFileList(Serialize(getClientHashtable(clientPath)),clientPath), typeof(Hashtable));
            if (obj==null) return null;
            return obj as Hashtable;
        
        }

        public Hashtable getClientHashtable(string path)
        {
            System.Collections.Hashtable hashtable = null;
            if (System.IO.File.Exists(getConfigFile(path)))
            {
                Object obj=Deserialize(getConfigFile(path), typeof(System.Collections.Hashtable));
                if(obj!=null)
                hashtable = (System.Collections.Hashtable)obj;
            }
            else
            {
                hashtable = new System.Collections.Hashtable();
            }
            return hashtable;
        }

        public String getClientPath()
        {
            return System.Windows.Forms.Application.StartupPath;
        }

        public String getClientTmpPath()
        {
            return Path.Combine(System.Windows.Forms.Application.StartupPath,"_Tmp4Update");
        }

        public String getClientFilePath(String relativePath)
        {
            return System.IO.Path.Combine(getClientPath(), relativePath);
        }

        public String getClientFileTmpPath(String relativePath)
        {
            return System.IO.Path.Combine(getClientTmpPath(), relativePath);
        }

        public void KillProcess(String processId)
        {

            Process process = null;
            try
            {
                process = Process.GetProcessById(Convert.ToInt32(processId));
            }
            catch { //如果进程已经正常中止，调用会抛出异常，此时不需要对此异常进行处理
                
            }
   
            if (process == null)  return;

            //if (System.Windows.Forms.MessageBox.Show("系统正在运行，关闭后才可以更新！", "系统更新"
            //    , System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
            //{
                try
                {
                    process.Kill();
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("停止当前程序失败，系统无法更新！");
                    //throw new Exception("kill process exception");
                }
            //}
            return;
        }

        public bool HasNewVersion(string wslParam,string path)
        {
            System.Collections.Hashtable clientHashtable = getClientHashtable(path);
            string clientVer = ((clientHashtable == null || !clientHashtable.ContainsKey(LAST_VER)) ? String.Empty : clientHashtable[LAST_VER] as string);
            return ServiceAgent.Instance(wslParam).getServerVers(clientVer,path).Length > 0;
        }

        public string BuildCmdLineParam() {

            StringBuilder sb = new StringBuilder();
            if (ConfigurationManager.AppSettings["proxy"] != null) sb.Append(" -proxy").Append(ConfigurationManager.AppSettings["proxy"]);
            if (ConfigurationManager.AppSettings["port"] != null) sb.Append(" -port").Append(ConfigurationManager.AppSettings["port"]);
            if (ConfigurationManager.AppSettings["userName"] != null) sb.Append(" -userName").Append(ConfigurationManager.AppSettings["userName"]);
            if (ConfigurationManager.AppSettings["userPwd"] != null) sb.Append(" -userPwd").Append(ConfigurationManager.AppSettings["userPwd"]);
            if (ConfigurationManager.AppSettings["domain"] != null) sb.Append(" -domain").Append(ConfigurationManager.AppSettings["domain"]);
            if (ConfigurationManager.AppSettings["Jointown.AutoUpdate.AutoUpdateLive.Service.FileService"] != null) sb.Append(" -Jointown.AutoUpdate.AutoUpdateLive.Service.FileService").Append(ConfigurationManager.AppSettings["Jointown.AutoUpdate.AutoUpdateLive.Service.FileService"]);
            return sb.ToString();            
        }
        public string BuildCmdLineParam(string url)
        {
            StringBuilder sb = new StringBuilder();
            if (ConfigurationManager.AppSettings["proxy"] != null) sb.Append(" -proxy").Append(ConfigurationManager.AppSettings["proxy"]);
            if (ConfigurationManager.AppSettings["port"] != null) sb.Append(" -port").Append(ConfigurationManager.AppSettings["port"]);
            if (ConfigurationManager.AppSettings["userName"] != null) sb.Append(" -userName").Append(ConfigurationManager.AppSettings["userName"]);
            if (ConfigurationManager.AppSettings["userPwd"] != null) sb.Append(" -userPwd").Append(ConfigurationManager.AppSettings["userPwd"]);
            if (ConfigurationManager.AppSettings["domain"] != null) sb.Append(" -domain").Append(ConfigurationManager.AppSettings["domain"]);
            if (url != null) sb.Append(" -Jointown.AutoUpdate.AutoUpdateLive.Service.FileService").Append(url);
            return sb.ToString();      
        }



        public string BuildCmdLineParam(string proxy,string port,string userName,string userPwd,string domain,string url)
        {

            StringBuilder sb = new StringBuilder();
            if (proxy != null) sb.Append(" -proxy").Append(proxy);
            if (proxy != null) sb.Append(" -port").Append(port);
            if (proxy != null) sb.Append(" -userName").Append(userName);
            if (proxy != null) sb.Append(" -userPwd").Append(userPwd);
            if (proxy != null) sb.Append(" -domain").Append(domain);
            if (url != null) sb.Append(" -Jointown.AutoUpdate.AutoUpdateLive.Service.FileService").Append(url);
            return sb.ToString();
        }

        public string RevertCmdLineParam(string key,string param)
        {
            string[] items=param.Split(' ');
            foreach(string item in items){
                if (item.Trim().IndexOf("-" + key) == 0) return item.Trim().Replace("-" + key, "");
            }
            return null;
        }

        public void CopyDirectory(string sourceDirName, string destDirName)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
            }

            if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
                destDirName = destDirName + Path.DirectorySeparatorChar;

            string[] files = Directory.GetFiles(sourceDirName);
            foreach (string file in files)
            {
                File.Copy(file, destDirName + Path.GetFileName(file), true);
                File.SetAttributes(destDirName + Path.GetFileName(file), FileAttributes.Normal);
            }
            string[] dirs = Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
            {
                CopyDirectory(dir, destDirName + Path.GetFileName(dir));
            }
        }   



	}
}
