# FCG Application

Este repositório contém a aplicação FCG, desenvolvida em .NET 9 e C# 13.0. A aplicação fornece APIs para gerenciamento de clientes, jogos e usuários, além de suporte a GraphQL.

---

## Tecnologias Utilizadas

- **.NET 9**
- **C# 13.0**
- **SQL Server** como banco de dados
- **Docker e Docker Compose** para containerização
- **GraphQL** para consultas avançadas
- **Swagger** para documentação da API
- **Serilog** para logging

---

## Pré-requisitos

Antes de executar a aplicação, certifique-se de ter instalado:
- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- SDK do .NET (opcional, para desenvolvimento local)

---

## Integração Contínua e Deploy Automático (GitHub Actions)

Este repositório utiliza o GitHub Actions para automatizar o processo de integração contínua (CI) e deploy. O workflow principal está definido em .github/workflows/main.yml e executa as seguintes etapas:

Build e Testes: Ao criar um pull request ou enviar commits para a branch master, o workflow realiza o build da aplicação e executa os testes automatizados usando .NET 9.
Build e Push Docker: Após os testes passarem, o workflow constrói as imagens Docker e faz o push para o Docker Hub.
Deploy no Azure: Finalmente, a aplicação é implantada automaticamente no Azure Web App utilizando a imagem Docker publicada.
Benefícios
Garantia de que o código enviado para a branch principal está sempre testado.
Implantação automática e rápida após cada alteração aprovada.
Facilidade para rastrear e reverter problemas através do histórico de builds e deploys.
Para mais detalhes, consulte o arquivo main.yml.

---

## Deploy no Azure Web App com Azure CLI

Execute os comandos abaixo para publicar sua aplicação containerizada no Azure e criar o banco SQL Server.  
Cada comando possui um comentário explicando sua função.

# 1. Cria um grupo de recursos chamado RGDockFiap na região eastUS
az group create --name RGDockFiap --location eastUS

# 2. Cria um App Service Plan para aplicações Linux
az appservice plan create -n myappservplanfcg -g RGDockFiap --is-linux

# 3. Cria o WebApp usando a imagem Docker do Docker Hub (altere o nome da imagem se necessário)
az webapp create -n webContainerAppFGC -g RGDockFiap -p myappservplanfcg -i dtpontes/fcgpresentation:latest

# 4. Cria um servidor SQL no Azure (altere usuário e senha para valores seguros)
az sql server create -l westus2 -g RGDockFiap -n fiapsqlserver5 -u sqladmin -p P2ssw0rd1234

# 5. Cria o banco de dados chamado mhcdb no servidor recém-criado
az sql db create -g RGDockFiap -s fiapsqlserver5 -n mhcdb --service-objective S0

# 6. Libera acesso ao SQL Server para serviços do Azure
az sql server firewall-rule create --resource-group RGDockFiap --server fiapsqlserver5 --name AllowAllAzureIps --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# 7. Define a string de conexão do banco de dados no WebApp
az webapp config connection-string set -g RGDockFiap -n webContainerAppFGC -t SQLAzure --settings defaultConnection='Data Source=tcp:fiapsqlserver5.database.windows.net,1433;Initial Catalog=mhcdb;User Id=sqladmin;Password=P2ssw0rd1234;'

---

## Executando a Aplicação com Docker Compose

A aplicação está configurada para ser executada utilizando Docker Compose. Siga os passos abaixo:

### Passo 1: Clonar o Repositório
Clone o repositório para sua máquina local:

git clone [https://github.com/dtpontes/FCG.git](https://github.com/dtpontes/FCG.git) cd seu-repositorio


### Passo 2: Construir e Executar os Containers
Execute o seguinte comando para construir e iniciar os containers:

docker-compose up --build


Este comando irá:
1. Construir as imagens Docker da aplicação.
2. Iniciar os containers definidos no arquivo `docker-compose.yml`.

### Passo 3: Acessar a Aplicação
Após os containers estarem em execução, você pode acessar a aplicação:
- **Swagger UI**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Endpoint GraphQL**: [http://localhost:8080/graphql](http://localhost:8080/graphql)

### Passo 4: Parar os Containers
Para parar os containers, execute:

docker-compose down

---

## Configuração

### Banco de Dados
A aplicação utiliza SQL Server como banco de dados. A string de conexão já está configurada no arquivo `docker-compose.yml` como variável de ambiente:

ConnectionStrings__DefaultConnection=Server=db;Database=FCGDatabase;User=sa;Password=YourStrongPassword123;TrustServerCertificate=True;


### Variáveis de Ambiente
Você pode configurar variáveis de ambiente adicionais no arquivo `docker-compose.override.yml` para fins de desenvolvimento.

---

## Solução de Problemas

Caso encontre problemas, verifique:
- Se o Docker está em execução.
- Se as portas 8080, 8081 e 1433 não estão bloqueadas por outros aplicativos.
- Se o container do banco de dados está saudável.

---

## Informações Adicionais

- A aplicação utiliza Serilog para logging.
- O Swagger está habilitado para documentação da API.
- O GraphQL está disponível para consultas de dados.

Para mais informações, consulte a documentação do projeto ou entre em contato com os mantenedores.
