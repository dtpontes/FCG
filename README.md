# FCG Application

Este repositório contém a aplicação FCG, desenvolvida em .NET 9 e C# 13.0. A aplicação fornece APIs para gerenciamento de clientes, jogos e usuários, com suporte a GraphQL, documentação interativa (Swagger) e logging estruturado (Serilog).

---

## 📑 Sumário

- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Pré-requisitos](#pré-requisitos)
- [Execução Local com Docker Compose](#execução-local-com-docker-compose)
- [Integração Contínua e Deploy Automático (GitHub Actions)](#integração-contínua-e-deploy-automático-github-actions)
- [Deploy no Azure](#deploy-no-azure)
---

## Tecnologias Utilizadas

- **.NET 9 / C# 13.0**
- **SQL Server** (banco de dados relacional)
- **Docker e Docker Compose** (containerização e orquestração)
- **GraphQL** (consultas avançadas)
- **Swagger** (documentação interativa)
- **Serilog** (logging estruturado)
- **New Relic** (monitoramento de performance)

---

## Pré-requisitos

- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- SDK do .NET (opcional, para desenvolvimento local)

---

## Execução Local com Docker Compose

1. **Clone o repositório:**

    ```sh
    git clone https://github.com/dtpontes/FCG.git
    cd FCG
    ```

2. **Construa e execute os containers:**

    ```sh
    docker-compose up --build
    ```

    Isso irá:
    - Construir as imagens Docker da aplicação.
    - Iniciar os containers conforme definido no `docker-compose.yml`.

3. **Acesse a aplicação:**
    - **Swagger UI:** [http://localhost:8080/swagger](http://localhost:8080/swagger)
    - **GraphQL Playground:** [http://localhost:8080/graphql](http://localhost:8080/graphql)

4. **Para parar os containers:**

    ```sh
    docker-compose down
    ```

---

## Integração Contínua e Deploy Automático (GitHub Actions)

Este repositório utiliza o **GitHub Actions** para automatizar processos de build, testes, criação e publicação de imagens Docker e deploy no Azure.

**Principais etapas do workflow (`.github/workflows/main.yml`):**
- **Build e Testes:** Ao criar um pull request ou enviar commits para a branch `master`, o workflow realiza o build da aplicação e executa testes automatizados.
- **Build e Push Docker:** Após os testes, a imagem Docker é construída e enviada para o Docker Hub.
- **Deploy no Azure:** A aplicação é implantada automaticamente no Azure Web App utilizando a imagem Docker publicada.

**Vantagens:**
- Garante que o código está sempre testado antes do deploy.
- Implantação automática e rápida após cada alteração aprovada.
- Facilidade para rastrear e reverter problemas via histórico de builds e deploys.

> Para detalhes ou customizações, consulte o arquivo [`main.yml`](.github/workflows/main.yml).

---

## Deploy no Azure

Execute os comandos abaixo para publicar sua aplicação no Azure Web App (com Docker) e criar o banco SQL Server.  
Cada comando possui um comentário explicando sua função.

```sh
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
