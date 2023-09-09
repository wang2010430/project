/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Class1.cs
* date      : 2023/7/9 14:35:08
* author    : jinlong.wang
* brief     : 
* section Modification History
* - 1.0 : Initial version - jinlong.wang
***************************************************************************************************/

namespace NVParam.DAL.NVTable
{
    public class NV_Project
    {
        public Common_RO Common_RO { get; set; } = new Common_RO();
        public Common_RW Common_RW { get; set; } = new Common_RW();
        public Calibration Calibration { get; set; } = new Calibration();
        public Modem_RF Modern_RF { get; set; } = new Modem_RF();
        public Modem_PHY Modern_PHY { get; set; } = new Modem_PHY();
        public Modem_PS Modern_PS { get; set; } = new Modem_PS();
        public Platform Platform { get; set; } = new Platform();
    }

    public class Common_RW
    {
        public modem_common modem_common { get; set; } = new modem_common();
       
    }

    public class Common_RO
    {
        
    }

    public class Modem_RF
    {
        
    }

    public class Modem_PHY
    {
        public phy_cfg phy_cfg { get; set; } = new phy_cfg();
    }

    public class Modem_PS
    {
    }

    public class Platform
    {
        
    }

    
}
