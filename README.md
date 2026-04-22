# AfroBarber Game

> Jogo de simulação de barbearia afro desenvolvido em Unity, com foco em atendimento, gestão, cultura negra, estética afro, educação sobre cortes/cabelos afro, progressão do jogador, loja, inventário, sistema financeiro, fila de clientes, energia, reputação e diálogos contextuais.

---

## 1. Visão geral do projeto

**AfroBarber Game** é um projeto de game de simulação em terceira pessoa, ambientado em uma barbearia com estética afro, cultura negra, identidade urbana e elementos ligados ao universo do hip-hop, da ancestralidade, da estética afro-brasileira e afro-americana. O jogador assume o papel de barbeiro/gestor da barbearia e precisa atender clientes, administrar recursos, comprar produtos e ferramentas, manter a reputação do estabelecimento, lidar com energia/cansaço, organizar a fila e desbloquear conteúdos educativos relacionados a cortes e estilos afro.

O projeto mistura três pilares principais:

1. **Simulação de barbearia**  
   O jogador gerencia uma barbearia funcional, recebe clientes, atende pedidos, usa ferramentas e produtos, recebe pagamentos, acumula dinheiro no caixa e compra novos itens para melhorar a qualidade dos atendimentos.

2. **Gameplay de atendimento e gestão**  
   Cada cliente possui um pedido de serviço, requisitos de produtos/ferramentas, tempo estimado, recompensa em dinheiro, recompensa de XP e possíveis vínculos com conteúdo educativo. O fluxo do cliente envolve entrada na barbearia, espera em assentos/sofás, entrada na fila, atendimento na cadeira, ida ao caixa e saída do estabelecimento.

3. **Educação e valorização cultural**  
   O jogo também funciona como uma experiência educativa, apresentando estilos de cabelo afro, suas histórias, significados culturais e relações com identidade, estética, comunidade e expressão pessoal.

A proposta é que o projeto possa evoluir para um jogo mobile/PC com uma experiência imersiva, organizada e expansível, permitindo que novas equipes ou novas IAs consigam entender a estrutura e continuar o desenvolvimento sem precisar reconstruir a lógica do zero.

---

## 2. Objetivo do README

Este README foi criado para documentar o projeto de forma completa, servindo como:

- documento de apresentação do projeto;
- guia de configuração em outro ambiente;
- mapa da arquitetura atual;
- referência para manutenção futura;
- guia para novas equipes de desenvolvimento;
- base para agentes de IA entenderem o projeto antes de propor alterações;
- checklist para reprodução do gameplay principal.

O foco não é apenas explicar o que existe, mas também deixar claro como os sistemas conversam entre si e quais objetos precisam existir na cena para tudo funcionar.

---

## 3. Estado atual do projeto

O projeto já possui diversos módulos implementados ou em evolução:

- NPCs/clientes com fluxo de entrada, espera, atendimento, caixa e saída.
- Sistema de assentos/sofás com reserva de lugares.
- Sistema de fila de atendimento.
- Sistema de pedidos de clientes com `ScriptableObjects`.
- Sistema de serviços com preço, tempo, XP e requisitos.
- Sistema de atendimento avançado com planejamento e execução por etapas.
- Sistema de inventário ligado aos produtos comprados.
- Sistema de loja com bancos de produtos por categoria.
- Sistema financeiro/caixa e UI de dinheiro.
- Sistema de energia/cansaço do jogador.
- Sistema de reputação/avaliação da barbearia.
- Sistema de histórico de atendimentos.
- Sistema educativo com desbloqueio de cortes/estilos.
- Sistema de diálogo global e mensagens contextuais no HUD.
- Sistema visual de cabelo do cliente antes/depois do atendimento.
- UIs para loja, inventário, atendimento, fila, educação, finanças, histórico e HUD.
- Arquivos obsoletos isolados em `_Deprecated`.

---

## 4. Conceito de gameplay

### 4.1 Loop principal

O loop principal esperado do jogo é:

1. Cliente nasce no ponto de spawn.
2. Cliente entra na barbearia.
3. Cliente vai até a área de espera.
4. Sistema reserva um assento disponível.
5. Cliente senta e entra na fila.
6. Jogador interage com o cliente ou chama o próximo da fila.
7. UI do pedido é exibida.
8. Jogador aceita ou dispensa o atendimento.
9. Sistema verifica se há ferramentas/produtos no inventário.
10. Cliente vai até a cadeira do barbeiro.
11. Cliente senta na cadeira.
12. Sistema abre planejamento de atendimento, quando habilitado.
13. Jogador escolhe/ordena etapas do atendimento.
14. Sistema executa o atendimento.
15. Resultado é avaliado.
16. Cliente recebe cabelo final, quando configurado.
17. Jogador recebe dinheiro, XP e avaliação.
18. Conteúdo educativo pode ser desbloqueado.
19. Produtos/ferramentas podem ser consumidos/desgastados.
20. Cliente vai ao caixa.
21. Cliente sai da barbearia.
22. Spawner libera o prefab para poder aparecer novamente.

### 4.2 Progressão

A progressão do jogador acontece por:

- dinheiro acumulado;
- compra de produtos e ferramentas melhores;
- XP do jogador;
- melhoria de reputação;
- desbloqueio de conteúdos educativos;
- ampliação da capacidade/qualidade da barbearia;
- aumento da eficiência nos atendimentos.

### 4.3 Gestão

A gestão aparece em decisões como:

- escolher quais produtos comprar;
- manter estoque suficiente;
- usar ferramentas adequadas;
- controlar o tempo dos atendimentos;
- manter a energia do barbeiro;
- decidir quando atender ou dispensar clientes;
- priorizar clientes na fila;
- equilibrar qualidade, velocidade e lucro.

---

## 5. Tecnologias e dependências

### 5.1 Engine

- Unity.
- Projeto orientado a componentes `MonoBehaviour`.
- Uso de `ScriptableObject` para dados de serviços, produtos, bancos e tabelas.
- Uso de `NavMeshAgent` para movimentação de NPCs.
- Uso de `Animator` para animações básicas como andar e sentar.
- Uso de TextMeshPro nas UIs.

### 5.2 Dependências esperadas no Unity

Antes de abrir/configurar o projeto em outro ambiente, garantir:

- TextMeshPro importado.
- Sistema de UI ativo.
- Canvas na cena.
- EventSystem na cena.
- Navigation/NavMesh configurado para NPCs.
- Tags corretas, especialmente `Player`.
- Layers/colliders organizados para cenário, player e NPCs.
- Pacotes de input/câmera conforme a versão atual do projeto.

### 5.3 Componentes comuns usados

- `Transform`
- `GameObject`
- `NavMeshAgent`
- `Animator`
- `Canvas`
- `Button`
- `Image`
- `TMP_Text`
- `ScrollRect`
- `ScriptableObject`

---

## 6. Estrutura geral de diretórios

A estrutura atual do repositório está organizada por domínios de gameplay e UI. Os principais diretórios são:

```text
Barbershop/
Characters/
Dialogue/
Economy/
Education/
Energy/
Evaluation/
Inventory/
NPC/
Products/
Queue/
Reputation/
ScriptableObjects/
Services/
UI/
Utilities/
_Deprecated/
```

Abaixo está o papel de cada área.

---

## 7. Diretório `Characters/Clients`

Contém os scripts ligados diretamente aos clientes/NPCs de atendimento.

### 7.1 `ClientNPC.cs`

É um dos scripts centrais do projeto. Controla o ciclo de vida do cliente dentro da barbearia.

Responsabilidades principais:

- guardar o estado atual do cliente;
- receber referências do spawner;
- ir até a entrada;
- reservar assento;
- ir até o ponto de espera;
- sentar no sofá;
- exibir ícone de interação;
- abrir UI de pedido quando clicado;
- chamar o serviço;
- ir até a cadeira do barbeiro;
- sentar na cadeira;
- aplicar cabelo inicial/final;
- ir ao caixa;
- sair da barbearia;
- remover cliente da fila;
- liberar assento;
- avisar o spawner quando foi destruído.

Estados atuais:

```csharp
None
GoingToEntrance
GoingToWaitingPoint
WaitingForService
GoingToBarberChair
InService
GoingToCashier
Leaving
```

Campos importantes no Inspector:

- `agent`: NavMeshAgent do cliente.
- `animator`: Animator do cliente.
- `interactionIcon`: ícone que aparece quando o cliente pode ser clicado.
- `hairVisualController`: controlador visual de cabelo.
- `serviceProfile`: perfil fixo do cliente.
- `clientDisplayName`: nome exibido do cliente.
- `arrivalDistance`: distância considerada chegada ao destino.
- `cashierWaitTime`: tempo parado no caixa antes de sair.
- `maxPatienceMinutes`: paciência usada no sistema de fila.
- `snapToSeatOnArrival`: força posição exata no assento.
- `rotateToSeatOnArrival`: força rotação do assento.
- `speedParam`: parâmetro de velocidade no Animator.
- `sitParam`: parâmetro booleano de sentar no Animator.

Métodos importantes:

- `Initialize(...)`: inicializa o cliente com referências da cena.
- `SetPlayerTransform(...)`: recebe referência do player.
- `SetRequest(...)`: define pedido manualmente.
- `OnPlayerClicked()`: abre UI do pedido.
- `CallForService()`: chama o manager de atendimento.
- `StartService(...)`: inicia ida para a cadeira.
- `MarkServiceCompleted()`: marca atendimento concluído e aplica cabelo final.
- `GoToCashier(...)`: leva cliente ao caixa.
- `LeaveShop(...)`: leva cliente ao ponto de saída.
- `ForceDespawn()`: destrói cliente com limpeza.

### 7.2 Configuração do prefab de cliente

Cada prefab de cliente deve conter:

- `ClientNPC`.
- `NavMeshAgent`.
- `Animator` configurado.
- Collider, se necessário para clique/proximidade.
- Ícone de interação como filho, inicialmente desativado.
- `ClientHairVisualController`, quando for usar troca visual de cabelo.
- `ClientServiceProfile`, se o cliente tiver pedido fixo.
- `NPCIdentity`, se for participar do sistema de diálogo/relação.
- `NPCRelationshipMemory`, se for registrar memórias.
- `NPCSocialProfile`, se for usar perfil social.
- `NPCConversationBrain`, se for gerar falas contextuais.

Animator recomendado:

- Float `Speed` para caminhada.
- Bool `Sit` para sentar.

---

## 8. Diretório `Barbershop`

Contém sistemas ligados à barbearia em si.

### 8.1 `BarbershopServiceManager.cs`

Manager principal do atendimento. Ele controla quando um cliente pode começar o serviço, valida energia, verifica inventário, prepara loadout, inicia atendimento antigo ou avançado, consome produtos, registra pagamento, XP, reputação e envia o cliente ao caixa/saída.

Responsabilidades:

- garantir apenas um cliente em atendimento por vez;
- validar se o cliente possui pedido;
- validar energia do jogador;
- validar pontos da cadeira;
- montar loadout automático com itens do inventário;
- bloquear atendimento se faltarem produtos/ferramentas;
- iniciar ida do cliente para a cadeira;
- abrir UI de planejamento avançado;
- executar atendimento avançado;
- usar atendimento automático antigo como fallback;
- consumir itens do inventário;
- aplicar pagamento;
- aplicar XP;
- aplicar avaliação/reputação;
- desbloquear conteúdo educativo;
- enviar cliente ao caixa ou saída.

Campos importantes:

- `servicePlanningUI`
- `openPlanningUIBeforeAdvancedExecution`
- `playerTransform`
- `planningInteractionPoint`
- `planningInteractionDistance`
- `barberChairWalkPoint`
- `barberChairSitPoint`
- `cashierPoint`
- `exitPoint`
- `barberWorkController`
- `advancedWorkflow`
- `useAdvancedServiceWorkflow`
- `fallbackToOldAutoServiceIfAdvancedFails`
- `consumeInventoryOnFinish`
- `sendClientAwayIfMissingItems`

Fluxo de `TryStartService(ClientNPC client)`:

1. Verifica cliente nulo.
2. Verifica se já existe cliente em atendimento.
3. Verifica se existe `RequestData`.
4. Verifica energia via `PlayerEnergySystem`.
5. Verifica ponto da cadeira.
6. Prepara loadout do pedido.
7. Verifica se loadout está completo.
8. Seta cliente atual.
9. Marca cliente como sendo atendido na fila.
10. Calcula qualidade de equipamento/produtos.
11. Inicia `BarberWorkController`.
12. Chama `client.StartService(...)`.
13. Aguarda cliente sentar.
14. Abre UI de planejamento ou executa fluxo.

### 8.2 Pontos necessários na cena

Para o atendimento funcionar, crie objetos vazios na cena:

```text
ClientSpawnPoint
EntrancePoint
BarberChairWalkPoint
BarberChairSitPoint
CashierPoint
ExitPoint
PlanningInteractionPoint
```

Recomendações:

- `BarberChairWalkPoint`: ponto acessível pelo NavMesh, próximo à cadeira.
- `BarberChairSitPoint`: posição exata onde o cliente ficará sentado.
- `CashierPoint`: ponto acessível pelo NavMesh em frente ao caixa.
- `ExitPoint`: ponto fora da barbearia ou na saída.
- `PlanningInteractionPoint`: ponto usado para medir se o player está perto o suficiente para abrir o planejamento.

---

## 9. Diretório `Barbershop/Waiting`

Controla a área de espera.

### 9.1 Conceito

O sistema de espera foi criado para evitar que vários NPCs tentem ocupar o mesmo sofá/lugar. Cada assento é reservado por um cliente. Quando o cliente é chamado ou vai embora, o assento é liberado.

### 9.2 Componentes esperados

- `WaitingAreaManager`
- `SofaSeatGroup`
- `WaitingSeat`

### 9.3 Estrutura recomendada no Hierarchy

```text
WaitingArea
├── Sofa_01
│   ├── Seat_01
│   │   ├── ApproachPoint
│   │   └── SitPoint
│   ├── Seat_02
│   │   ├── ApproachPoint
│   │   └── SitPoint
│   └── Seat_03
│       ├── ApproachPoint
│       └── SitPoint
├── Sofa_02
│   └── ...
└── Sofa_03
    └── ...
```

### 9.4 Diferença entre ApproachPoint e SitPoint

- `ApproachPoint`: ponto navegável no chão, onde o NavMeshAgent consegue chegar.
- `SitPoint`: ponto exato do corpo sentado, geralmente em cima/na frente do sofá.

Isso evita o bug do NPC bater no sofá ou sentar fora do lugar. O agente caminha até o `ApproachPoint`, depois o script encaixa o cliente no `SitPoint`.

---

## 10. Diretório `Queue`

Sistema de fila da barbearia.

### 10.1 `BarberQueueSystem.cs`

Controla a lista de clientes esperando atendimento. Permite adicionar, remover e marcar clientes como em atendimento.

Responsabilidades:

- manter dados de fila;
- controlar paciência/tempo de espera;
- fornecer dados para UI;
- marcar cliente como sendo atendido;
- remover cliente ao sair ou iniciar atendimento.

### 10.2 `ClientQueueData.cs`

Modelo de dados de cada cliente na fila. Normalmente armazena:

- referência do cliente;
- nome do cliente;
- tempo máximo de paciência;
- status de atendimento;
- tempo de espera.

### 10.3 UI da fila

Arquivos relacionados:

```text
UI/Queue/QueueClientCardUI.cs
UI/Queue/QueueClientCard.prefab
```

A UI deve listar os clientes aguardando, permitindo futuramente ações como chamar próximo, observar paciência, priorizar ou dispensar.

---

## 11. Diretório `Services`

Contém os dados e sistemas de serviços/pedidos.

### 11.1 `ClientRequestData`

Representa um pedido de cliente. Deve conter dados como:

- id do pedido;
- nome do pedido;
- descrição;
- tipo de serviço;
- preço;
- tempo de atendimento;
- recompensa de XP;
- requisitos de produtos/ferramentas;
- id educativo do corte;
- cabelo antes;
- cabelo depois.

Os pedidos ficam em:

```text
ScriptableObjects/Services/Requests/
```

Exemplos identificados:

```text
Request_BlackPower.asset
Request_DreadsLocs.asset
Request_FadeAfro.asset
Request_FlatTop.asset
Request_Twists.asset
```

### 11.2 Banco de pedidos

Arquivo principal:

```text
ScriptableObjects/Services/Databases/MainClientRequestDatabase.asset
```

Script relacionado:

```text
Services/ClientRequestDatabase.cs
```

Uso esperado:

- cadastrar todos os pedidos disponíveis;
- permitir que spawner/clientes sorteiem pedidos;
- centralizar a lista de serviços do jogo.

### 11.3 Requisitos de serviço

Script:

```text
Services/ServiceRequirementData.cs
```

Cada requisito representa uma necessidade do atendimento, por exemplo:

- máquina de corte;
- tesoura;
- pente;
- navalha;
- lâmina;
- produto capilar;
- secador;
- item por serviço;
- item por hora de uso.

Campos comuns esperados:

- `requirementId`;
- categoria necessária;
- quantidade consumida;
- horas consumidas;
- tipo de uso;
- nome de exibição.

### 11.4 Tabela de preços

Arquivos relacionados:

```text
Services/PriceTable/MainServicePriceTable.asset
Services/ServicePriceEntry.cs
```

A tabela de preços centraliza valores padrão por tipo de serviço. Isso evita que cada pedido tenha preço aleatório sem padrão.

Serviços sugeridos pela proposta do projeto:

- Corte de Cabelo;
- Corte de Cabelo e Barba;
- Barba;
- Acabamento/Pezinho;
- Design de Sobrancelhas;
- Tranças/Twists/Dreads/Black Power, conforme evolução do jogo.

---

## 12. Diretório `Services/Advanced`

Sistema de atendimento avançado.

### 12.1 Conceito

O atendimento avançado permite que o serviço seja dividido em etapas, com planejamento e execução. Em vez de apenas esperar um timer, o jogador pode montar um plano de atendimento usando ferramentas compatíveis.

### 12.2 Arquivos principais

```text
Services/Advanced/AdvancedServiceWorkflowManager.cs
Services/Advanced/AdvancedServiceModels.cs
Services/Advanced/AdvancedServiceOutcomeResolver.cs
Services/Advanced/ServiceExecutionSystem.cs
Services/Advanced/ServiceToolCompatibility.cs
```

### 12.3 `AdvancedServiceWorkflowManager.cs`

Responsabilidades:

- criar plano para o cliente;
- guardar plano por cliente;
- escolher ferramentas compatíveis;
- validar se há plano;
- executar plano;
- aplicar resultado;
- adicionar dinheiro;
- adicionar XP;
- ajustar relacionamento com NPC;
- registrar memórias;
- enviar mensagens ao chat global.

Etapas padrão adicionadas automaticamente:

```csharp
Wash
Cut
Finish
Finalize
```

### 12.4 Compatibilidade de ferramentas

`ServiceToolCompatibility` define quais categorias de produto podem ser usadas em cada ação. Exemplo:

- cortar exige máquina/tesoura/navalha;
- lavar/finalizar pode exigir produto capilar;
- acabamento pode exigir navalha/lâmina.

### 12.5 Resultado avançado

`AdvancedServiceOutcomeResolver` calcula o resultado final considerando:

- plano usado;
- qualidade das ferramentas;
- tempo real;
- ações concluídas;
- erros;
- compatibilidade;
- requisitos do pedido.

O resultado pode gerar:

- nota final;
- dinheiro;
- XP;
- avaliação;
- mensagens de feedback.

---

## 13. Diretório `Products`

Contém a estrutura de produtos da loja e inventário.

### 13.1 `ProductData`

Representa um produto comprável/usável. Deve conter:

- id do produto;
- nome;
- categoria;
- tipo de item de inventário;
- preço;
- ícone;
- precisão;
- velocidade;
- durabilidade;
- quantidade/usos;
- prefab opcional;
- descrição.

Atributos usados em gameplay:

- `precisao`: influencia qualidade do atendimento.
- `velocidade`: influencia tempo/eficiência.
- `durabilidade`: influencia resistência/vida útil.

### 13.2 Categorias

Bancos identificados:

```text
ScriptableObjects/Products/Databases/DB_MaquinasCorte.asset
ScriptableObjects/Products/Databases/DB_Tesouras.asset
ScriptableObjects/Products/Databases/DB_Pentes.asset
ScriptableObjects/Products/Databases/DB_Navalhas.asset
ScriptableObjects/Products/Databases/DB_Laminas.asset
ScriptableObjects/Products/Databases/DB_ProdutosCapilar.asset
ScriptableObjects/Products/Databases/DB_Secador.asset
ScriptableObjects/Products/Databases/DB_Mobilia.asset
ScriptableObjects/Products/Databases/DB_Decoracao.asset
```

### 13.3 `ProductDatabase.cs`

Agrupa produtos de uma categoria ou base de produtos. Usado pela loja/inventário para listar itens.

### 13.4 `CategoryDatabaseEntry.cs`

Liga uma categoria a um banco específico. Isso permite que a loja tenha bancos separados por categoria.

Exemplo conceitual:

```text
MaquinasCorte -> DB_MaquinasCorte
Tesouras      -> DB_Tesouras
Pentes        -> DB_Pentes
Navalhas      -> DB_Navalhas
```

### 13.5 `ProductDurabilitySystem.cs`

Controla desgaste/durabilidade dos produtos, principalmente itens duráveis como máquinas, tesouras e secadores.

---

## 14. Diretório `Inventory`

Sistema de inventário do jogador/barbearia.

### 14.1 `InventoryManager.cs`

É o manager central dos itens comprados/possuídos.

Responsabilidades:

- armazenar produtos comprados;
- retornar todos os itens possuídos;
- buscar `ProductData` por id;
- consumir usos por item único;
- consumir horas de uso;
- remover itens inutilizáveis;
- fornecer dados para atendimento avançado;
- fornecer dados para UI de inventário.

Métodos usados por outros sistemas:

- `GetAllOwnedItems()`
- `GetProductDataById(...)`
- `ConsumeProductUsageByUniqueId(...)`
- `ConsumeDurableHoursByUniqueId(...)`
- `RemoveAllUnusableItems()`

### 14.2 Relação com atendimento

Quando o cliente inicia atendimento:

1. O `BarbershopServiceManager` lê os requisitos do pedido.
2. O `ServiceLoadoutBuilder` tenta montar um loadout com itens do inventário.
3. Se os itens forem suficientes, o atendimento começa.
4. Ao finalizar, o manager consome usos ou horas dos itens usados.
5. Itens inutilizáveis são removidos.

---

## 15. Diretório `UI/Shop` e loja

A loja permite comprar produtos para uso no inventário e nos atendimentos.

### 15.1 Conceito

A loja deve listar produtos por categoria, mostrar dados de cada item e permitir compra usando dinheiro do jogador/caixa.

### 15.2 Arquivos importantes

```text
UI/Shop/
Products/
ScriptableObjects/Products/
```

Prefabs identificados:

```text
UI/Shop/MenuItemButtom.prefab
```

### 15.3 Configuração recomendada da loja

Na cena, criar uma UI com:

```text
ShopPanel
├── Header
│   └── CashText
├── CategoryMenu
│   ├── Button_Maquinas
│   ├── Button_Tesouras
│   ├── Button_Pentes
│   └── ...
└── ProductList
    └── Content
```

O manager da loja deve receber:

- lista de bancos por categoria;
- prefab de card de produto;
- content onde os cards serão instanciados;
- referência ao inventário;
- referência ao dinheiro/caixa;
- texto do valor atual do caixa.

### 15.4 Integração com dinheiro

A loja deve debitar do caixa quando o usuário compra. O texto do cabeçalho deve ser atualizado automaticamente por evento ou binder.

---

## 16. Diretório `Economy`

Contém scripts ligados a dinheiro, carteira, caixa e exibição monetária.

### 16.1 `PlayerWallet`

Representa a carteira/caixa atual usado por alguns fluxos, principalmente atendimento avançado.

Uso encontrado:

```csharp
PlayerWallet wallet = Object.FindFirstObjectByType<PlayerWallet>();
wallet.AddMoney(result.moneyReward);
```

### 16.2 `FinanceManager`

Responsável por registrar entradas financeiras, principalmente renda por serviço no fluxo antigo/financeiro.

Uso encontrado no atendimento:

```csharp
FinanceManager.Instance.RegisterServiceIncome(request, currentClientName);
```

### 16.3 `MoneyTextBinder.cs`

Componente de UI para ligar um `TMP_Text` ao valor atual de dinheiro/caixa.

### 16.4 Atenção sobre duplicidade financeira

O projeto já teve versões antigas de dinheiro em `_Deprecated`, como `PlayerMoney`, e há sistemas atuais como `PlayerWallet`, `FinanceManager` e possivelmente caixa da barbearia. Para manutenção futura, é recomendável consolidar uma fonte principal de verdade.

Recomendação:

- usar `FinanceManager` como histórico financeiro;
- usar `PlayerWallet` ou caixa central como saldo atual;
- evitar que scripts em `_Deprecated` gravem no mesmo `saveKey`;
- remover ou isolar qualquer referência antiga a `AFROBARBER_PLAYER_MONEY` se não for mais usada.

---

## 17. Diretório `UI/Finance`

Interface de finanças.

Arquivos/prefabs identificados:

```text
UI/Finance/Prefabs/FinanceMovementRow.prefab
```

A UI financeira deve mostrar:

- saldo atual;
- entradas;
- saídas;
- movimentos por data;
- agrupamento mensal;
- total de entradas do mês;
- total de saídas do mês;
- saldo do mês;
- saldo positivo em verde;
- saldo negativo em vermelho.

---

## 18. Diretório `Energy`

Sistema de energia, trabalho e descanso.

### 18.1 Arquivos principais

```text
Energy/PlayerEnergySystem.cs
Energy/BarberWorkController.cs
Energy/PlayerExhaustionController.cs
Energy/RestUIController.cs
Energy/CarRestPoint.cs
```

### 18.2 Conceito

O barbeiro possui energia. Atendimentos consomem energia e podem ficar bloqueados se o jogador estiver cansado.

O `BarbershopServiceManager` consulta:

```csharp
PlayerEnergySystem.Instance.CanStartService()
```

E também usa multiplicadores de tempo:

```csharp
PlayerEnergySystem.Instance.GetServiceTimeMultiplier()
```

### 18.3 `BarberWorkController`

Controla informações do serviço atual e influencia duração/qualidade do atendimento com base em energia e condições.

Responsabilidades:

- receber nome e preço do serviço atual;
- iniciar atendimento;
- calcular duração ajustada;
- finalizar atendimento;
- gerar avaliação.

### 18.4 Descanso

`RestUIController` e `CarRestPoint` indicam que existe ou está previsto um sistema onde o jogador pode descansar para recuperar energia.

---

## 19. Diretório `Evaluation`

Sistema de avaliação do atendimento.

Arquivos principais:

```text
Evaluation/ClientEvaluationSystem.cs
Evaluation/ServiceEvaluationResult.cs
Evaluation/ServiceSessionData.cs
```

A avaliação deve considerar:

- tempo esperado;
- tempo real;
- qualidade do equipamento;
- qualidade dos produtos;
- conforto do ambiente;
- erros durante o atendimento;
- resultado final.

O resultado pode alimentar:

- reputação;
- feedback visual;
- histórico de atendimento;
- satisfação do cliente;
- recompensas futuras.

---

## 20. Diretório `Reputation`

Sistema de reputação da barbearia.

Arquivos identificados:

```text
Reputation/ServiceHistorySystem.cs
Reputation/ReputationUI.cs
```

A reputação é atualizada a partir das avaliações dos atendimentos.

Uso encontrado no manager:

```csharp
BarbershopRatingManager.Instance.AddReview(rating);
```

A UI de reputação deve exibir:

- nota média;
- quantidade de avaliações;
- histórico;
- impacto da qualidade dos serviços.

---

## 21. Diretório `Education`

Sistema educativo.

### 21.1 `EducationProgressManager.cs`

Controla o desbloqueio de conteúdos educativos relacionados aos cortes/estilos.

O atendimento chama:

```csharp
EducationProgressManager.Instance.UnlockCut(request.afroCutId);
```

E o cliente pode consultar:

```csharp
EducationProgressManager.Instance.GetCutById(currentRequest.afroCutId);
```

### 21.2 UI educativa

Arquivo identificado:

```text
UI/Education/EducationEncyclopediaItemUI.cs
```

A proposta da enciclopédia educativa é apresentar:

- nome do corte;
- descrição;
- origem/história;
- significado cultural;
- relação com estética afro;
- status desbloqueado/bloqueado.

### 21.3 Integração com pedidos

Cada `ClientRequestData` pode conter `afroCutId`. Quando o serviço é concluído, esse id é desbloqueado.

---

## 22. Diretório `Dialogue`

Sistema global de diálogo, histórico de conversa e falas contextuais.

### 22.1 Arquivos principais

```text
Dialogue/DialogueModels.cs
Dialogue/DialogueContextResolver.cs
Dialogue/DialogueOptionGenerator.cs
Dialogue/DialogueReactionEvaluator.cs
Dialogue/GlobalDialogueManager.cs
Dialogue/NPCConversationBrain.cs
Dialogue/NPCDialogueRepertoire.cs
```

### 22.2 `GlobalDialogueManager.cs`

Manager global do diálogo. Deve existir uma única instância ativa na cena. Pode usar singleton e permanecer entre cenas.

Funções esperadas:

- registrar mensagens do sistema;
- registrar mensagens de NPC;
- manter histórico;
- limitar quantidade armazenada;
- notificar UI quando chegam mensagens;
- permitir contexto de gameplay nas mensagens.

Exemplos de uso encontrados:

```csharp
GlobalDialogueManager.Instance?.AddNpcMessage(identity, "Beleza, pode montar meu atendimento.", DialogueContextType.Service);
GlobalDialogueManager.Instance?.AddSystemMessage("Atendimento finalizado...", DialogueContextType.Service);
```

### 22.3 HUD de chat

Arquivos identificados:

```text
UI/HUD/Chat/HudChatMessageItemUI.cs
UI/HUD/Chat/DialogueOptionsButtonUI.cs
```

A proposta do HUD de chat:

- mostrar histórico das conversas realizadas;
- não permitir digitar mensagens livremente;
- mostrar opções contextuais de fala;
- permitir ações de gameplay a partir das opções;
- exibir mensagens espontâneas de NPCs;
- piscar ícone do NPC quando ele falar sem o jogador iniciar.

### 22.4 Opções contextuais

Exemplos de opções quando o jogador está na barbearia com clientes:

- “Próximo da fila”;
- “Hoje não vou atender mais ninguém”;
- “Aguarde só mais um pouco”;
- “Pode sentar na cadeira”.

Cada fala deve acionar uma ação real de gameplay.

---

## 23. Diretório `NPC`

Contém dados sociais e identidade dos NPCs.

Arquivos identificados:

```text
NPC/NPCIdentity.cs
NPC/NPCRelationshipMemory.cs
NPC/NPCSocialProfile.cs
NPC/NPCInteractionIndicator.cs
```

### 23.1 `NPCIdentity`

Guarda identidade do NPC, nome e possivelmente relação com o jogador.

### 23.2 `NPCRelationshipMemory`

Permite guardar memórias de interações, por exemplo:

- resultado de atendimento;
- satisfação;
- eventos importantes;
- impacto positivo/negativo no relacionamento.

### 23.3 `NPCSocialProfile`

Pode representar personalidade, estilo social ou perfil do NPC.

### 23.4 `NPCInteractionIndicator`

Controla indicadores visuais de interação, como ícones acima do NPC.

---

## 24. Sistema de cabelo dos clientes

Arquivos identificados:

```text
Characters/Clients/ClientHairDefinition.cs
Characters/Clients/ClientHairVisualController.cs
```

### 24.1 Conceito

Cada cliente pode ter um cabelo inicial e um cabelo final após o atendimento.

A troca pode ser definida por:

- `beforeHairId` no pedido;
- `afterHairId` no pedido;
- perfil fixo do cliente;
- definições cadastradas no controlador visual.

### 24.2 Fluxo

Na inicialização do cliente:

```text
ApplyInitialHair()
```

Ao concluir serviço:

```text
ApplyFinalHair()
```

### 24.3 Configuração prática

No prefab do cliente:

1. Criar objetos filhos para os cabelos possíveis.
2. Desativar todos por padrão, exceto o inicial se desejar.
3. Adicionar `ClientHairVisualController`.
4. Cadastrar ids dos cabelos.
5. No `ClientRequestData`, preencher `beforeHairId` e `afterHairId`.
6. Testar se ao iniciar o cliente aparece o cabelo inicial.
7. Testar se ao concluir o serviço troca para o cabelo final.

---

## 25. Diretório `UI/ClientRequest`

Interface do pedido do cliente.

Arquivos identificados:

```text
UI/ClientRequest/ProductChoiceItemUI.cs
UI/ClientRequest/RequestRequirementSlotUI.cs
UI/ClientRequest/RequestRequirementSlot.prefab
```

A UI do pedido deve mostrar:

- nome do cliente;
- nome do pedido;
- descrição do serviço;
- preço;
- XP;
- tempo estimado;
- requisitos;
- produtos disponíveis para cada requisito;
- botão aceitar;
- botão fechar/dispensar;
- informações educativas do corte, quando aplicável.

### 25.1 Fluxo de abertura

1. Cliente chega ao sofá.
2. Estado vira `WaitingForService`.
3. Ícone de interação aparece.
4. Jogador clica no cliente.
5. `ClientNPC.OnPlayerClicked()` chama `ClientRequestUI.Instance.Show(this)`.
6. UI carrega dados do `ClientNPC.RequestData`.
7. Jogador aceita.
8. Cliente chama `CallForService()`.

### 25.2 Problemas comuns

Se aparecer erro:

```text
ClientRequestUI.Instance não encontrado.
```

Verificar:

- existe objeto com `ClientRequestUI` na cena;
- o objeto está ativo ou o singleton inicializa mesmo inativo;
- não existem múltiplas instâncias;
- referências de botões e textos foram preenchidas;
- Canvas e EventSystem existem.

---

## 26. Diretório `UI/Inventory`

Interface de inventário.

Arquivos/prefabs identificados:

```text
UI/Inventory/InventoryCategoryGroupUI.cs
UI/Inventory/Prefabs/ProductCard.prefab
UI/Inventory/Prefabs/CategoryCard.prefab
```

A UI de inventário deve mostrar:

- categorias de itens;
- produtos possuídos;
- quantidade/usos restantes;
- durabilidade;
- status utilizável/inutilizável;
- atributos do produto;
- ícone;
- descrição.

---

## 27. Diretório `UI/HUD`

Interface principal do jogador durante o gameplay.

Arquivos identificados:

```text
UI/HUD/GameTimeUI.cs
UI/HUD/EnergyUI.cs
UI/HUD/Chat/HudChatMessageItemUI.cs
UI/HUD/Chat/DialogueOptionsButtonUI.cs
```

HUD deve conter:

- dinheiro/caixa atual;
- energia;
- horário/data do jogo;
- botão de loja;
- botão de inventário;
- botão de financeiro;
- botão de histórico/notificações;
- card de chat/histórico de conversas;
- indicador de atendimento atual;
- opção de abrir planejamento quando cliente estiver na cadeira.

---

## 28. Sistema de tempo

Arquivos identificados:

```text
UI/HUD/GameTimeUI.cs
Utilities/Debug/GameTimeDebugUI.cs
Utilities/Debug/TimeSkyboxTester.cs
```

O sistema de tempo deve exibir o horário do jogo e futuramente se integrar com:

- abertura/fechamento da barbearia;
- iluminação;
- skybox;
- fluxo de clientes;
- cansaço;
- encerramento do expediente;
- clientes indo embora ao fim do dia.

---

## 29. Diretório `_Deprecated`

Contém arquivos antigos que não devem ser usados como fonte principal do projeto.

Arquivos identificados:

```text
_Deprecated/SofaSeats.cs
_Deprecated/ShopPurchaseHandler.cs
```

Também houve versões antigas de dinheiro/financeiro em conversas anteriores.

Regras de manutenção:

- não adicionar scripts novos em `_Deprecated`;
- não referenciar scripts deprecated em cenas atuais;
- manter apenas como histórico temporário;
- remover definitivamente quando a arquitetura estiver estável;
- se algum script deprecated ainda for necessário, migrar para o diretório correto antes de usar.

---

## 30. Configuração completa em outro ambiente

### 30.1 Clonar o repositório

```bash
git clone https://github.com/marcelo-braga-dev/afrobarber.git
```

Depois abrir a pasta pelo Unity Hub.

### 30.2 Conferir versão do Unity

Ao abrir em outro computador:

1. Abrir Unity Hub.
2. Selecionar o projeto.
3. Usar a versão compatível com a que foi usada no desenvolvimento.
4. Se o Unity pedir upgrade, fazer backup antes.
5. Aguardar importação completa dos assets.

### 30.3 Importar TextMeshPro

No Unity:

```text
Window > TextMeshPro > Import TMP Essential Resources
```

### 30.4 Conferir cenas

A cena principal deve conter:

```text
Canvas
EventSystem
Player
Main Camera
Global Managers
BarbershopServiceManager
WaitingAreaManager
ClientSpawner
InventoryManager
FinanceManager/PlayerWallet
PlayerEnergySystem
GlobalDialogueManager
EducationProgressManager
BarberQueueSystem
```

### 30.5 Configurar Navigation/NavMesh

1. Marcar chão como navegável.
2. Marcar obstáculos corretamente.
3. Gerar/Bake NavMesh.
4. Garantir que os pontos de destino fiquem sobre o NavMesh.
5. Garantir que `ApproachPoint`, `EntrancePoint`, `BarberChairWalkPoint`, `CashierPoint` e `ExitPoint` sejam alcançáveis.

### 30.6 Configurar Player

O player deve ter:

- tag `Player`;
- controlador de movimento;
- câmera configurada;
- collider/CharacterController;
- scripts de interação, se usados;
- referência para sistemas que exigem playerTransform.

### 30.7 Configurar Canvas/UI

Criar ou conferir:

- HUD principal;
- UI de pedido do cliente;
- UI de planejamento de serviço;
- UI de execução avançada;
- UI de avaliação;
- UI de loja;
- UI de inventário;
- UI de fila;
- UI de financeiro;
- UI educativa;
- UI de energia;
- HUD de chat.

Todas as UIs com botões precisam de `EventSystem`.

---

## 31. Configuração dos managers globais

### 31.1 Objeto `GameManagers`

Recomendado criar um objeto vazio:

```text
GameManagers
├── BarbershopServiceManager
├── AdvancedServiceWorkflowManager
├── ServicePlanningSystem
├── ServiceExecutionSystem
├── InventoryManager
├── FinanceManager
├── PlayerWallet
├── PlayerEnergySystem
├── EducationProgressManager
├── BarberQueueSystem
├── GlobalDialogueManager
└── BarbershopRatingManager
```

Pode ser um único GameObject com vários componentes ou objetos separados.

### 31.2 Ordem de atenção

Sistemas com `Instance` precisam existir antes de serem chamados:

- `BarbershopServiceManager.Instance`
- `InventoryManager.Instance`
- `FinanceManager.Instance`
- `PlayerEnergySystem.Instance`
- `EducationProgressManager.Instance`
- `BarberQueueSystem.Instance`
- `GlobalDialogueManager.Instance`
- `PlayerXPManager.Instance`
- `BarbershopRatingManager.Instance`

---

## 32. Configuração do spawner de clientes

O `ClientSpawner` deve receber:

- lista de prefabs de clientes;
- ponto de spawn;
- ponto de entrada;
- `WaitingAreaManager`;
- ponto da cadeira/barbearia via manager;
- ponto de saída;
- banco de pedidos;
- referência do player;
- intervalo de spawn;
- controle para não repetir o mesmo prefab simultaneamente;
- delay de respawn após cliente sair.

Fluxo esperado do spawner:

1. Escolhe prefab disponível.
2. Instancia no spawn point.
3. Escolhe pedido fallback no banco, se o cliente não tiver perfil fixo.
4. Chama `ClientNPC.Initialize(...)`.
5. Cliente executa seu ciclo.
6. Quando sai, chama `NotifyClientFinished(...)`.
7. Spawner libera prefab.

---

## 33. Configuração de serviços/pedidos

### 33.1 Criar novo pedido

No Unity:

1. Criar um novo `ClientRequestData`.
2. Preencher id único.
3. Preencher nome do pedido.
4. Preencher descrição.
5. Selecionar tipo de serviço.
6. Definir preço ou vincular tabela de preços.
7. Definir tempo de atendimento.
8. Definir XP.
9. Adicionar requisitos.
10. Definir `afroCutId`, se houver conteúdo educativo.
11. Definir `beforeHairId` e `afterHairId`, se houver troca visual.
12. Adicionar ao `MainClientRequestDatabase.asset`.

### 33.2 Criar requisito

Para cada requisito:

1. Definir id único.
2. Definir categoria necessária.
3. Definir tipo de uso: por serviço ou por hora.
4. Definir quantidade consumida.
5. Definir nome de exibição.
6. Salvar como asset ou configurar dentro do pedido.

### 33.3 Testar pedido

Checklist:

- pedido aparece na UI do cliente;
- preço aparece corretamente;
- tempo aparece corretamente;
- XP aparece corretamente;
- requisitos aparecem na UI;
- o inventário reconhece itens compatíveis;
- atendimento é bloqueado se faltam itens;
- atendimento inicia se há itens;
- produtos são consumidos ao final;
- dinheiro/XP são aplicados;
- conteúdo educativo é desbloqueado.

---

## 34. Configuração de produtos

### 34.1 Criar produto

1. Criar novo `ProductData`.
2. Definir id único.
3. Definir nome.
4. Definir categoria.
5. Definir tipo de inventário.
6. Definir preço.
7. Definir ícone.
8. Definir precisão.
9. Definir velocidade.
10. Definir durabilidade.
11. Definir quantidade/usos.
12. Definir descrição.
13. Adicionar ao banco da categoria correta.

### 34.2 Adicionar produto à loja

1. Abrir banco da categoria.
2. Inserir produto na lista.
3. Abrir cena da loja.
4. Verificar se o botão da categoria aponta para o banco correto.
5. Rodar o jogo.
6. Abrir loja.
7. Conferir se o item aparece.
8. Comprar.
9. Conferir se aparece no inventário.
10. Conferir se pode ser usado no atendimento.

---

## 35. Configuração de inventário

O `InventoryManager` precisa conhecer os bancos de produtos para conseguir resolver `productId` em `ProductData`.

Checklist:

- `InventoryManager` existe na cena.
- Todos os bancos de produtos estão cadastrados.
- Produtos têm ids únicos.
- Produtos comprados geram estados únicos no inventário.
- Itens duráveis possuem durabilidade/usos.
- Itens consumíveis possuem quantidade/usos.
- O atendimento consegue buscar itens compatíveis.

---

## 36. Configuração de atendimento avançado

### 36.1 Objetos necessários

Na cena:

```text
AdvancedServiceWorkflowManager
ServicePlanningSystem
ServiceExecutionSystem
ServicePlanningUI
AdvancedServiceExecutionUI
ServiceEvaluationUI
```

### 36.2 No `BarbershopServiceManager`

Configurar:

- `useAdvancedServiceWorkflow = true`
- `openPlanningUIBeforeAdvancedExecution = true`
- `fallbackToOldAutoServiceIfAdvancedFails = true`, para segurança durante desenvolvimento
- `advancedWorkflow`
- `servicePlanningUI`
- `planningInteractionPoint`
- `planningInteractionDistance`

### 36.3 Teste

1. Spawnar cliente.
2. Cliente senta no sofá.
3. Abrir pedido.
4. Aceitar atendimento.
5. Cliente vai até cadeira.
6. Jogador fica perto da cadeira.
7. UI de planejamento abre.
8. Plano é montado.
9. Atendimento executa etapas.
10. UI de execução mostra progresso.
11. UI de avaliação aparece.
12. Cliente vai ao caixa.
13. Cliente sai.

---

## 37. Configuração de finanças

### 37.1 Entrada por serviço

No fluxo antigo, `BarbershopServiceManager` chama:

```csharp
FinanceManager.Instance.RegisterServiceIncome(request, currentClientName);
```

No fluxo avançado, `AdvancedServiceWorkflowManager` chama:

```csharp
wallet.AddMoney(result.moneyReward);
```

### 37.2 Recomendação de arquitetura

Para evitar divergência:

- toda entrada/saída deve passar por um serviço financeiro único;
- a UI deve ouvir eventos do saldo;
- a loja deve debitar pelo mesmo sistema;
- o atendimento deve creditar pelo mesmo sistema;
- histórico financeiro deve registrar a origem do movimento.

### 37.3 Movimentos esperados

Entradas:

- pagamento de atendimento;
- bônus;
- recompensas.

Saídas:

- compra de produtos;
- compra de móveis;
- manutenção;
- contas/despesas futuras.

---

## 38. Configuração de diálogo/HUD chat

### 38.1 Objeto global

Criar:

```text
GlobalDialogueManager
```

Adicionar componente `GlobalDialogueManager`.

Configurar:

- máximo de mensagens armazenadas;
- logs em console para debug;
- integração com HUD.

### 38.2 HUD chat

Criar painel:

```text
HudChatCard
├── Scroll View
│   └── Content
├── OptionsButton
└── NotificationIndicator
```

O card deve:

- listar mensagens;
- rolar automaticamente;
- mostrar mensagens do sistema e NPCs;
- permitir abrir opções contextuais;
- não permitir digitação livre.

### 38.3 Mensagens espontâneas

NPCs podem falar mesmo sem clique do jogador. Nesses casos:

- mensagem aparece no chat;
- popup aparece na tela;
- ícone do NPC pisca;
- jogador pode iniciar conversa/interação depois.

---

## 39. Configuração educativa

### 39.1 Banco de cortes

Criar ou configurar banco com cortes/estilos afro.

Cada item deve conter:

- id;
- nome;
- descrição;
- história;
- significado cultural;
- imagem/ícone;
- status desbloqueado.

### 39.2 Ligação com pedidos

No pedido, preencher:

```text
afroCutId
```

Ao concluir atendimento, o conteúdo é desbloqueado.

---

## 40. Configuração de energia

### 40.1 Objeto necessário

Criar:

```text
PlayerEnergySystem
```

Configurar:

- energia máxima;
- energia inicial;
- gasto por atendimento;
- recuperação;
- multiplicador de tempo quando cansado;
- bloqueio de atendimento sem energia.

### 40.2 UI de energia

Adicionar `EnergyUI` ao HUD e ligar:

- barra de energia;
- texto de energia;
- alertas de cansaço.

---

## 41. Configuração da reputação

Criar manager de reputação/avaliação:

```text
BarbershopRatingManager
ServiceHistorySystem
ReputationUI
```

O atendimento deve registrar avaliações após o serviço.

A UI deve mostrar:

- avaliação média;
- histórico;
- última avaliação;
- comentários futuros de clientes.

---

## 42. Checklist de cena funcional mínima

Para uma cena simples funcionar, ela precisa ter:

```text
[Ambiente]
- Chão com NavMesh
- Obstáculos com colliders
- Cadeira de barbeiro
- Sofá/área de espera
- Caixa
- Porta/saída

[Player]
- Player com tag Player
- Controlador de movimento
- Collider/CharacterController
- Câmera

[NPC]
- Prefabs de clientes com ClientNPC
- NavMeshAgent
- Animator
- Ícone de interação

[Pontos]
- SpawnPoint
- EntrancePoint
- WaitingSeat ApproachPoints
- WaitingSeat SitPoints
- BarberChairWalkPoint
- BarberChairSitPoint
- CashierPoint
- ExitPoint

[Managers]
- ClientSpawner
- WaitingAreaManager
- BarbershopServiceManager
- InventoryManager
- FinanceManager ou PlayerWallet
- PlayerEnergySystem
- BarberQueueSystem
- EducationProgressManager
- GlobalDialogueManager
- AdvancedServiceWorkflowManager

[UI]
- Canvas
- EventSystem
- HUD
- ClientRequestUI
- ServicePlanningUI
- AdvancedServiceExecutionUI
- ServiceEvaluationUI
- ShopUI
- InventoryUI
- QueueUI
- FinanceUI
- ChatHUD
```

---

## 43. Fluxo de teste completo

Após configurar tudo:

1. Entrar no Play Mode.
2. Confirmar que não há erros no Console.
3. Verificar se cliente nasce.
4. Verificar se cliente vai até a entrada.
5. Verificar se cliente reserva assento.
6. Verificar se cliente caminha até `ApproachPoint`.
7. Verificar se cliente encaixa no `SitPoint`.
8. Verificar se animação `Sit` ativa.
9. Verificar se ícone de interação aparece.
10. Clicar no cliente.
11. Confirmar que UI de pedido abre.
12. Confirmar que requisitos aparecem.
13. Aceitar atendimento.
14. Confirmar que cliente sai da fila/assento.
15. Confirmar que cliente vai para cadeira.
16. Confirmar que cliente senta na cadeira.
17. Confirmar que UI de planejamento abre.
18. Executar atendimento.
19. Confirmar que avaliação aparece.
20. Confirmar dinheiro/XP.
21. Confirmar consumo de produtos.
22. Confirmar desbloqueio educativo.
23. Confirmar troca de cabelo final.
24. Confirmar ida ao caixa.
25. Confirmar saída do cliente.
26. Confirmar que spawner libera novo cliente.

---

## 44. Problemas comuns e correções

### 44.1 Cliente não anda

Verificar:

- NavMesh foi gerado.
- Cliente está sobre NavMesh.
- Destination está sobre NavMesh.
- `NavMeshAgent` está ativo.
- Obstáculos não bloqueiam caminho.
- Agent radius/height compatíveis.

### 44.2 Cliente fica rodando perto do ponto

Correções:

- aumentar `arrivalDistance`;
- usar `ApproachPoint` no chão, não em cima do sofá;
- reduzir obstáculos perto do ponto;
- conferir se `stoppingDistance` do agente está compatível;
- garantir que `SitPoint` seja usado apenas para snap final.

### 44.3 Cliente senta fora do sofá

Verificar:

- `ApproachPoint` está no chão alcançável;
- `SitPoint` está exatamente onde o corpo deve ficar;
- rotação do `SitPoint` está correta;
- `snapToSeatOnArrival` está ativo;
- `rotateToSeatOnArrival` está ativo.

### 44.4 UI do pedido não abre

Verificar:

- cliente está em `WaitingForService`;
- `ClientRequestUI.Instance` existe;
- Canvas existe;
- EventSystem existe;
- botão/click está chegando no `ClientNPC.OnPlayerClicked()`;
- cliente tem `RequestData`;
- não há CanvasGroup bloqueando clique.

### 44.5 Atendimento não inicia

Verificar:

- `BarbershopServiceManager.Instance` existe;
- não existe outro cliente em atendimento;
- cliente tem pedido;
- player tem energia;
- `barberChairWalkPoint` está configurado;
- inventário possui itens exigidos;
- loadout está completo.

### 44.6 Planejamento não abre

Verificar:

- `useAdvancedServiceWorkflow` ativo;
- `openPlanningUIBeforeAdvancedExecution` ativo;
- `AdvancedServiceWorkflowManager` existe;
- `ServicePlanningUI` existe;
- cliente já está `InService`;
- player está perto da cadeira;
- `planningInteractionPoint` está configurado.

### 44.7 Dinheiro não atualiza

Verificar:

- qual sistema está sendo usado: `FinanceManager` ou `PlayerWallet`;
- UI está ligada ao sistema correto;
- evento de atualização está disparando;
- fluxo avançado e antigo não estão usando fontes diferentes sem sincronização.

### 44.8 Produto comprado não aparece no inventário

Verificar:

- produto está no banco correto;
- produto tem id único;
- loja chama método de adicionar ao inventário;
- inventário conhece o banco daquele produto;
- UI do inventário atualiza após compra.

### 44.9 Atendimento bloqueado por falta de itens

Verificar:

- requisitos do pedido estão corretos;
- categorias dos produtos batem com requisitos;
- produto está no inventário, não apenas na loja;
- item ainda tem usos/durabilidade;
- compatibilidade está registrada em `ServiceToolCompatibility`.

---

## 45. Boas práticas de manutenção

### 45.1 Manter sistemas separados

Evitar misturar:

- UI com regra de negócio;
- dinheiro com inventário;
- atendimento com spawner;
- diálogo com avaliação;
- dados com lógica de cena.

### 45.2 Usar ScriptableObjects para dados

Sempre que for conteúdo configurável, preferir `ScriptableObject`:

- produtos;
- pedidos;
- serviços;
- cortes educativos;
- tabelas de preço;
- perfis de cliente;
- bancos de categoria.

### 45.3 IDs únicos

Manter ids únicos para:

- produtos;
- pedidos;
- requisitos;
- cortes educativos;
- cabelos;
- clientes especiais.

### 45.4 Evitar Find em excesso

O projeto usa alguns `FindFirstObjectByType` para fallback. Para produção, prefira referências configuradas no Inspector ou injeção via managers.

### 45.5 Evitar duplicidade de singleton

Managers com `Instance` devem existir uma única vez.

### 45.6 Manter `_Deprecated` limpo

Não usar scripts obsoletos em cenas atuais.

---

## 46. Convenções recomendadas

### 46.1 Nome de assets

```text
Request_NomeDoServico.asset
DB_NomeDaCategoria.asset
Product_NomeDoProduto.asset
UI_NomeDaTela.prefab
NPC_NomeDoCliente.prefab
Hair_NomeDoCabelo.prefab
```

### 46.2 Nome de pontos

```text
Point_ClientSpawn
Point_Entrance
Point_BarberChair_Walk
Point_BarberChair_Sit
Point_Cashier
Point_Exit
Point_PlanningInteraction
Seat_01_Approach
Seat_01_Sit
```

### 46.3 Nome de parâmetros Animator

```text
Speed
Sit
IsWalking
IsSitting
```

---

## 47. Roadmap recomendado

### 47.1 Curto prazo

- Consolidar sistema financeiro em uma única fonte de verdade.
- Garantir que loja, atendimento e HUD usem o mesmo saldo.
- Finalizar UI de planejamento avançado.
- Finalizar UI de avaliação.
- Melhorar HUD de chat com opções contextuais.
- Adicionar notificações globais.
- Melhorar configuração dos prefabs de cliente.

### 47.2 Médio prazo

- Adicionar sistema de expediente/horário comercial.
- Fazer clientes irem embora por falta de paciência.
- Melhorar reputação com comentários.
- Adicionar despesas mensais.
- Adicionar upgrades de barbearia.
- Criar mais cortes educativos.
- Melhorar animações de atendimento.

### 47.3 Longo prazo

- Sistema completo de cidade ao redor da barbearia.
- NPCs andando em calçadas e atravessando faixas.
- Eventos especiais.
- Campanha/progressão narrativa.
- Personalização visual da barbearia.
- Publicação mobile.

---

## 48. Orientação para novas IAs ou novos desenvolvedores

Antes de alterar qualquer código:

1. Leia este README.
2. Identifique qual sistema será alterado.
3. Verifique se já existe manager para esse domínio.
4. Não crie duplicidade de sistema.
5. Não altere `_Deprecated`, exceto para remoção/migração.
6. Verifique dependências de UI e Inspector.
7. Mantenha compatibilidade com `ClientNPC`, `BarbershopServiceManager`, `InventoryManager` e `ClientRequestData`.
8. Teste o fluxo completo de cliente após mudanças.
9. Documente qualquer novo script neste README.

---

## 49. Resumo executivo para apresentação

O **AfroBarber Game** é um jogo de simulação e gestão de barbearia afro em desenvolvimento na Unity. O projeto combina gameplay de atendimento ao cliente, administração de recursos, compra de produtos, controle de energia, reputação, fila de espera, execução de serviços e educação cultural sobre cabelos e estilos afro. A proposta é valorizar a estética negra e a cultura afro por meio de uma experiência interativa, onde cada atendimento pode ensinar o jogador sobre cortes, história, identidade e expressão cultural.

O jogador administra uma barbearia, recebe clientes com pedidos específicos, utiliza produtos e ferramentas adequadas, realiza atendimentos, ganha dinheiro e XP, melhora sua reputação e desbloqueia conteúdos educativos. O sistema foi estruturado com dados configuráveis por `ScriptableObjects`, permitindo expansão de produtos, serviços, cortes, clientes e conteúdos sem necessidade de reescrever toda a lógica.

O projeto já conta com uma arquitetura modular dividida em sistemas de clientes, atendimento, fila, espera, inventário, loja, finanças, energia, reputação, educação, diálogos e interface. Essa organização permite manutenção contínua, expansão futura e colaboração por novas equipes de desenvolvimento.

---

## 50. Status final

Este README representa o estado atual conhecido do projeto no repositório `marcelo-braga-dev/afrobarber`, incluindo a arquitetura principal, sistemas já desenvolvidos, estrutura de diretórios, configuração de cena, fluxo de gameplay, integrações e recomendações de manutenção.

Sempre que novos sistemas forem criados ou alterados, este arquivo deve ser atualizado para manter o projeto constante, reproduzível, compreensível e fácil de manter.
