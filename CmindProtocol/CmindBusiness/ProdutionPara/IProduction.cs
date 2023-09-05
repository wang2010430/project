/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : IProduction.cs
* date      : 2023/07/11
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/

namespace CmindProtocol.CmindBusiness.ProdutionPara
{
    public interface IProduction
    {
        ProductPhyStrutType StructType { get; }

        byte[] GetBytes();
    }
}
