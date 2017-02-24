using MsgPack.Serialization;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using System.Linq;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        MqttClient mc;

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mc = new MqttClient("192.168.2.74", 1883, false, null);//服务器ip,端口(需要做到客户端配置文件里)
            mc.Connect("", "admin", "admin");//客户端id(hello01改用一门式系统的用户id),登陆账号和密码（此项目前固定值）

            //以下两句代码，陈哲不需要用到，只作为测试数据是否有正常返回
            mc.ConnectionClosed += Mc_ConnectionClosed;
            mc.MqttMsgPublishReceived += Mc_MqttMsgPublishReceived;
            mc.Subscribe(new string[] { "5ad6df8d-cdd0-4459-b71c-15ffce5b4bdc/1/6" }, new byte[] { 0 }); //test1改为public时对应用的值
        }

        private void Mc_ConnectionClosed(object sender, EventArgs e)
        {
            //MessageBox.Show("建立连接失败");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // They are object for just description. 
            var targetObject = new Data
            {
                val = 666
            };
            var stream = new MemoryStream();
            var context = new SerializationContext();
            context.SerializationMethod = SerializationMethod.Map;
            // 1. Create serializer instance.
            var serializer = MessagePackSerializer.Get<Data>(context);

            // 2. Serialize object to the specified stream.
            serializer.Pack(stream, targetObject);


            //数据
            //var data = new Data() { userid = 112, affairInfoId = "skdjfkdjfksdf" };
            //判断是否已经和服务器建立连接
            if (mc.IsConnected)
            {
                //发送内容（test1改为一门式用户id）
                var pdata = stream.ToArray();
                mc.Publish("$SYS/broker/http/10b35668-b375-445b-adc0-279c413fa155/1/5/post/1", pdata, 0, false);
                //System.Linq.Enumerable.Range(0, 10000).AsParallel().ForAll(x =>
                //{
                //    mc.Publish("$SYS/broker/http/10b35668-b375-445b-adc0-279c413fa155/1/5/post/1", pdata, 0, false);
                //});
            }
            else
            {
                MessageBox.Show("没有和服务器建立连接");
            }
        }

        //陈哲不需要用到，只作为测试数据是否有正常返回
        private void Mc_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // 1. Create serializer instance.
            var serializer = MessagePackSerializer.Get<result>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });
            // 3. Deserialize object from the specified stream.
            var deserializedObject = serializer.Unpack(new MemoryStream(e.Message));
            Console.WriteLine("收到消息:" +Newtonsoft.Json.JsonConvert.SerializeObject(deserializedObject));
        }
    }

    public class result
    {
        public int Svalue { get; set; }
    }

    [MessagePackRuntimeDictionaryKeyType]
    public class Data
    {
        public int val { get; set; }
        // 更多...
    }

}
