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

## Executando a Aplicação com Docker Compose

A aplicação está configurada para ser executada utilizando Docker Compose. Siga os passos abaixo:

### Passo 1: Clonar o Repositório
Clone o repositório para sua máquina local:

git clone [https://github.com/seu-repositorio.git](https://github.com/dtpontes/FCG.git) cd seu-repositorio


### Passo 2: Construir e Executar os Containers
Execute o seguinte comando para construir e iniciar os containers:

docker-compose up --build


Este comando irá:
1. Construir as imagens Docker da aplicação.
2. Iniciar os containers definidos no arquivo `docker-compose.yml`.

### Passo 3: Acessar a Aplicação
Após os containers estarem em execução, você pode acessar a aplicação:
- **Swagger UI**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Endpoint GraphQL**: [http://localhost:8081/graphql](http://localhost:8081/graphql)

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
