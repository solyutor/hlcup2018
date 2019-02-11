using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace aspnetapp
{
    public static class WarmUpWorker
    {
        public static void Run()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost")
            };

            var requests = File.ReadAllLines("warmup.txt");
            var sw = Stopwatch.StartNew();
            Task.WaitAll(requests.Select(x => client.GetAsync(x)).ToArray());

            client.PostAsync("/accounts/new/?query_id=1", new StringContent(InvalidNewQuery, Encoding.UTF8, "application/json")).Wait();
            client.PostAsync("/accounts/likes/?query_id=1", new StringContent(InvalidLikes, Encoding.UTF8, "application/json")).Wait();
            client.PostAsync("/accounts/100/?query_id=1", new StringContent(InvalidUpdate, Encoding.UTF8, "application/json")).Wait();

            client.Dispose();
            sw.Stop();
            Console.WriteLine($"Warm up finished in {sw.Elapsed}");



        }

        public static string InvalidUpdate = "{\"sname\":\"\u0424\u0435\u0442\u043e\u043b\u043e\u043a\u0430\u044f\",\"status\":\"\u0441\u0432\u043e\u0431\u043e\u0434\u043d\u044bdsf151f\"}";

        public static string InvalidLikes = "{\"likes\":[{\"likee\":3315,\"ts\":1516729330,\"liker\":24808},{\"likee\":28790,\"ts\":1517194241,\"liker\":26163},{\"likee\":21193,\"ts\":1454540512,\"liker\":28336},{\"likee\":24366,\"ts\":1483102826,\"liker\":26163},{\"likee\":24867,\"ts\":1492531304,\"liker\":28336},{\"likee\":11437,\"ts\":1457978754,\"liker\":28336},{\"likee\":6596,\"ts\":1525002667,\"liker\":12199},{\"likee\":17610,\"ts\":1480272198,\"liker\":26163},{\"likee\":3748,\"ts\":1467737798,\"liker\":37},{\"likee\":26823,\"ts\":1522671412,\"liker\":968},{\"likee\":29503,\"ts\":1500090704,\"liker\":2920},{\"likee\":18387,\"ts\":1521487956,\"liker\":2920},{\"likee\":7078,\"ts\":1521205966,\"liker\":37},{\"likee\":18700,\"ts\":1531284480,\"liker\":20333},{\"likee\":3127,\"ts\":1462565042,\"liker\":968},{\"likee\":26014,\"ts\":1471659438,\"liker\":37},{\"likee\":22197,\"ts\":1488957380,\"liker\":968},{\"likee\":12923,\"ts\":1477912739,\"liker\":25276},{\"likee\":19084,\"ts\":1517797227,\"liker\":37},{\"likee\":23419,\"ts\":1528049791,\"liker\":2920},{\"likee\":27386,\"ts\":1507200542,\"liker\":26163},{\"likee\":26172,\"ts\":1492044774,\"liker\":20333},{\"likee\":18521,\"ts\":1477487617,\"liker\":24808},{\"likee\":10926,\"ts\":1463175456,\"liker\":37},{\"likee\":6944,\"ts\":1476431258,\"liker\":26163},{\"likee\":27013,\"ts\":1510785053,\"liker\":968},{\"likee\":6282,\"ts\":1475436901,\"liker\":1103},{\"likee\":25334,\"ts\":1518268572,\"liker\":1103},{\"likee\":7356,\"ts\":1531740653,\"liker\":20333},{\"likee\":19437,\"ts\":1493621260,\"liker\":24808},{\"likee\":17076,\"ts\":1478766499,\"liker\":1103},{\"likee\":3249,\"ts\":1473386568,\"liker\":2920},{\"likee\":29235,\"ts\":1487789509,\"liker\":25276},{\"likee\":8304,\"ts\":1489324045,\"liker\":26163},{\"likee\":28509,\"ts\":1459396246,\"liker\":7626},{\"likee\":2172,\"ts\":1526313380,\"liker\":37},{\"likee\":21546,\"ts\":1486245385,\"liker\":37},{\"likee\":17150,\"ts\":1493577514,\"liker\":20333},{\"likee\":26953,\"ts\":1468450567,\"liker\":968},{\"likee\":11556,\"ts\":1463138608,\"liker\":37},{\"likee\":12095,\"ts\":1454285393,\"liker\":24808},{\"likee\":256,\"ts\":1507071014,\"liker\":20333},{\"likee\":27199,\"ts\":1521649550,\"liker\":24808},{\"likee\":24612,\"ts\":1466815618,\"liker\":20333},{\"likee\":25942,\"ts\":1513056034,\"liker\":0}]}";

        public static string InvalidNewQuery = "{\"likes\":[{\"ts\":1458532958,\"id\":2068},{\"ts\":1530543279,\"id\":8192},{\"ts\":1504195330,\"id\":14222},{\"ts\":1531801936,\"id\":13052},{\"ts\":1483571968,\"id\":436},{\"ts\":1477798464,\"id\":13798},{\"ts\":1540794608,\"id\":27366},{\"ts\":1501938063,\"id\":2640},{\"ts\":1475711148,\"id\":18730},{\"ts\":1483182988,\"id\":1438},{\"ts\":1502868081,\"id\":20254},{\"ts\":1457154892,\"id\":5898},{\"ts\":1499109873,\"id\":27434},{\"ts\":1492498978,\"id\":3906},{\"ts\":1492727181,\"id\":148},{\"ts\":1455009416,\"id\":5130},{\"ts\":1499787738,\"id\":20974},{\"ts\":1457697787,\"id\":18186},{\"ts\":1468773125,\"id\":27452},{\"ts\":1514133181,\"id\":23554},{\"ts\":1482863630,\"id\":5190},{\"ts\":1459079122,\"id\":4274},{\"ts\":1529525143,\"id\":28170},{\"ts\":1501810072,\"id\":20680},{\"ts\":1454398908,\"id\":17898},{\"ts\":1499489038,\"id\":19682},{\"ts\":1507146242,\"id\":5252},{\"ts\":1520412653,\"id\":18584},{\"ts\":1507117971,\"id\":7142},{\"ts\":1475659744,\"id\":12788},{\"ts\":1527026490,\"id\":10682},{\"ts\":1455581885,\"id\":5518},{\"ts\":1501760787,\"id\":21064}],\"email\":\"tifiminisugirwedy@ymail.com\",\"birth\":642681139,\"status\":\"\u0432\u0441\u0451 \u0441\u043b\u043e\u0436\u043d\u043e\",\"joined\":1344124800,\"sex\":\"m\",\"interests\":[\"\u041d\u0430 \u043e\u0442\u043a\u0440\u044b\u0442\u043e\u043c \u0432\u043e\u0437\u0434\u0443\u0445\u0435\",\"\u0412\u0435\u0447\u0435\u0440 \u0441 \u0434\u0440\u0443\u0437\u044c\u044f\u043c\u0438\"],\"sname\":\"\u0424\u0430\u0430\u0442\u043e\u0432\u0438\u0447\",\"phone\":\"8(918)5543777\",\"country\":\"\u0420\u043e\u0441\u043b\u044f\u043d\u0434\u0438\u044f\",\"premium\":{\"start\":1519546195,\"finish\":1527408595},\"fname\":\"\u041d\u0438\u043a\u0438\u0442\u0430\"}";
    }
}