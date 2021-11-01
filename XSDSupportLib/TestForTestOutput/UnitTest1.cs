using AR430;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Xunit;

namespace TestForTestOutput
{
    public class UnitTest1
    {
        string[] FileLocation { get; set; } = new string[]
            {
                @"C:\Personal\WorkSpaces\PTC\P280\P280_SW_Dev\P280_SW\bk\01_AppLyr\Sys_Ctrl\ComWrp\arch\ComWrp_ComWrapper_ArItf.arxml",
                @"C:\Personal\WorkSpaces\PTC\P280\P280_SW_Dev\P280_SW\bk\02_Rte\Sys_cfg\arch\System_Template.arxml",
                @"C:\vector\CBD1900759_D00_Tricore\BSWMD\Dcm\Dcm_bswmd.arxml",
                @"C:\Personal\WorkSpaces\PTC\P200\Baseline\P200_B0501P04_1.28\pf\02_Rte\Ecu_cfg\arch\EcuExtract\FlatExtract.arxml"
            };
        [Fact]
        public void TestForConcatItem()
        {
            AUTOSARCollection collection = AUTOSARCollection.LoadFile(FileLocation);
            collection.GetElementArrayByType(out ApplicationSwComponentType[] aswComps);
            var res=AUTOSARCollection.ConcatItem(aswComps.Where(asw => asw.ToString().Equals("ComWrp")).ToArray());
            var res2 = collection.GetConcatResultByPath("/ComWrp_ArItf/ComWrp_DTyps/AD_ComWrp_tEDRVDES_S");
        }
        [Fact]
        public void TestForSearchingElementByType()
        {
            try
            {
                AR430.Autosar.GetInstance(@"C:\Personal\WorkSpaces\PTC\P200\Baseline\P200_B0501P04_1.28\pf\02_Rte\Ecu_cfg\arch\EcuExtract\FlatExtract.arxml", out AR430.Autosar autosarPackage);
                autosarPackage.GetElementArrayByType(out AR430.FibexElementRefConditional[] items);
            }
            catch (Exception e)
            {

            }
        }
        [Fact]
        public void TestForIdxHandler()
        {
            foreach (var loc in FileLocation)
            {
                try
                {
                    AR430.Autosar.GetInstance(loc, out AR430.Autosar autosarPackage);
                    var res = autosarPackage["ComWrp_ArItf"]["ComWrp"];
                    res = autosarPackage["System_Template"]["I_SIGNALS"];
                }
                catch (Exception e)
                {

                }
            }
        }
        [Fact]
        public void TestForFileLoading()
        {

            foreach (var loc in FileLocation)
            {
                try
                {
                    AR430.Autosar.GetInstance(loc, out AR430.Autosar autosarPackage);
                }
                catch (Exception e)
                {

                }
            }            
        }
        [Fact]
        public void TestLocation()
        {
            AR430.Autosar.GetInstance("U:\\DflsM.arxml", out AR430.Autosar autosarPackage);
            autosarPackage.GetElementByPath("/DflsM_ArItf/DflsM/DflsM_InternalBehavior/DflsM_AppValidWrite");
        }
    }
}
