using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RefactoringInjecaoDeDependencia.Controllers;
using RefactoringInjecaoDeDependencia.Dtos;
using RefactoringInjecaoDeDependencia.Repositories;
using RefactoringInjecaoDeDependencia.UnitTest;

namespace RefactoringInjecaoDependencia.UnitTest
{
    [TestClass]
    [TestCategory("UnitTest > Controllers > Preco")]
    public class PrecosControllerTest
    {
        [TestMethod]
        public void TesteMockComValorEspecifico()
        {
            var prodMock = new Mock<IProdutoRepository>();
            prodMock.Setup(m => m.Obter(It.IsAny<int>())).Returns((ProdutoDto)null);
            prodMock.Setup(m => m.Obter(1)).Returns(new ProdutoDto(33, "Novo Produto", 1022.33m, false));
            prodMock.Setup(m => m.Obter(0)).Returns(new ProdutoDto(33, "Novo Produto", 1000m, false));
            prodMock.Setup(m => m.Obter(0)).Returns((ProdutoDto)null);
            var taxaMock = new Mock<ITaxaEntregaRepository>();
            var precoCtrl = new PrecosController(prodMock.Object, taxaMock.Object);

            var res = precoCtrl.ObterPreco(1, 111111);

            var objRes = (ObterPrecoResponse)((OkObjectResult)res).Value;

            Assert.AreEqual(0, objRes.ValorEntrega);
            Assert.AreEqual(1022.33m, objRes.PrecoTotal);
            Assert.AreEqual(1022.33m, objRes.ValorProduto);
        }

        [TestMethod]
        public void DeveriaRetornarErroSeNaoEncontrarOProduto()
        {
            var prodMock = new Mock<IProdutoRepository>();
            prodMock.Setup(m => m.Obter(It.IsAny<int>())).Returns((ProdutoDto)null);
            var taxaMock = new Mock<ITaxaEntregaRepository>();
            var precoCtrl = new PrecosController(prodMock.Object, taxaMock.Object);
            
            var res = precoCtrl.ObterPreco(0, 111111);
            
            Assert.AreEqual(404, ((NotFoundObjectResult)res).StatusCode);
        }

        [TestMethod]
        public void DeveriaRetornarPrecoTotalIgualAValorProduto()
        {
            var prodMock = new Mock<IProdutoRepository>();
            prodMock.Setup(m => m.Obter(It.IsAny<int>())).Returns(new ProdutoDto(33, "Novo Produto", 1022.33m, false));
            var taxaMock = new Mock<ITaxaEntregaRepository>();
            var precoCtrl = new PrecosController(prodMock.Object, taxaMock.Object);
            
            var res = precoCtrl.ObterPreco(33, 75021010);
            var objRes = (ObterPrecoResponse)((OkObjectResult)res).Value;

            Assert.AreEqual(0, objRes.ValorEntrega);
            Assert.AreEqual(1022.33m, objRes.PrecoTotal);
            Assert.AreEqual(1022.33m, objRes.ValorProduto);
        }

        [TestMethod]
        public void DeveriaRetornarValorEntregaEPrecoTotalIgualASomaDoProdutoEEntrega()
        {
            var prodMock = new Mock<IProdutoRepository>();
            prodMock.Setup(m => m.Obter(It.IsAny<int>())).Returns(new ProdutoDto(1, "TV 75 polegadas", 7044.75m, true));
            var taxaMock = new Mock<ITaxaEntregaRepository>();
            taxaMock.Setup(m => m.Obter(It.IsAny<int>())).Returns(new TaxaEntregaDto(75021000, 75022010, 0.03m));

            var precoCtrl = new PrecosController(prodMock.Object, taxaMock.Object);

            var res = precoCtrl.ObterPreco(1, 75021010);
            var objRes = (ObterPrecoResponse)((OkObjectResult)res).Value;

            taxaMock.Verify(m => m.Obter(It.IsAny<int>()), Times.Exactly(1)); // ou Times.Once

            Assert.AreEqual(211.34m, objRes.ValorEntrega);
            Assert.AreEqual(7044.75m, objRes.ValorProduto);
            Assert.AreEqual(7256.09m, objRes.PrecoTotal);
        }
    }
}
