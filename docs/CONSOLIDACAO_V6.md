# Consolidação V6 — 26/10/2026

Base do projeto: Unity 6000.4.12f1, preservando os mapas e os sistemas existentes. Nenhuma nova arte, história, cutscene ou puzzle foi produzida nesta consolidação.

## Cenas oficiais

1. Assets/Scenes/MainMenu.unity
2. Assets/Scenes/Quarto.unity
3. Assets/V65Environments/Scenes/Labirinto.unity — 14 setores
4. Assets/V65Environments/Scenes/DominioLua.unity — 8 setores
5. Assets/Scenes/FinalBeta.unity

As cinco cenas foram copiadas sem alterações de bytes da V6 verificada. As cenas antigas de Labirinto e DominioLua em Assets/Scenes foram arquivadas fora do jogo por dependerem de sprites sem uso atual. Apenas as duas cenas integradas devem ser usadas. A cena V53_LibraryReview também foi arquivada.

## Manutenção

Assets, Packages, ProjectSettings e arquivos .meta mantêm os caminhos e GUIDs. Por instrução explícita do usuário, sprites só permanecem com uso comprovado no jogo atual. A auditoria final encontrou 815 texturas Sprite utilizadas e zero sem uso comprovado. Foram apagados do projeto 457 sprites sem uso e 116 assets serializados antigos que dependiam deles; o pacote externo de recuperação e os backups preservam esses arquivos. Uso futuro genérico não justifica reter um sprite. A remoção foi feita pela API da Unity e não alterou as cinco cenas oficiais.

Os dados requeridos por V65EnvironmentImporter ficam em Tools/EnvironmentMaintenance. Texturas já importadas são as fontes locais; JSONs de navegação e insumos de alvenaria também estão nesta pasta. As ferramentas não dependem de C:/Projetos/MementoMori-V6-Alternativa ou de arquivos pessoais do Codex.

Relatórios de Editor: TestResults/EnvironmentAudit e TestResults/Consolidation, ignorados pelo Git. Sondas opcionais de Play Mode escrevem em Application.persistentDataPath/Validation/EnvironmentAudit. Elas são ferramentas de teste e não equivalem a um playthrough humano completo.

Os três bakers da geração V6 anterior foram arquivados fora do jogo, pois operavam sobre os mapas substituídos. Não executar ferramentas de reconstrução artística para alterar os mapas aprovados.

## Evidências e recuperação

Backups SHA-256 separados e bundle Git completo: C:/Projetos/MementoMori-Consolidacao-20261026/Backups.

Inventários, classificação de assets, manifesto de limpeza e resultados atuais: C:/Projetos/MementoMori-Consolidacao-20261026/Auditoria.

Acervo acadêmico, com 16 categorias e arquivos deduplicados: C:/Users/Usuario/Documents/MementoMori_TCC_Evidencias/Consolidacao_20261026.

40 EditMode e 16 PlayMode aprovados antes da migração são resultados históricos. Consultar o relatório atual de consolidação para os testes realizados depois da migração. Nenhuma build Windows definitiva é prometida nesta etapa.

## Histórico e autoria

O checkout local e a branch remota de integração tinham históricos sem ancestral comum. Ambos foram preservados por uma integração sem reescrita e sem force push. O remote é https://github.com/mesferatus/MementoMori-GameDev.git, pertencente a Callisto.

Identidades comprovadas por registros Git: Callisto Monteiro Mascoloti <cmmascoloti@gmail.com> e Luiza Canova <lucanoliveira05@gmail.com>. Há também registros anteriores com o nome completo Luiza de Oliveira Canova e o mesmo e-mail.

A autoria da integração técnica atual representa a consolidação solicitada por Callisto, com assistência do Codex. A geometria visual aprovada deriva da contribuição registrada de Luiza no commit d61d74f9bcb553d524d94a72350f7f938729710a. Reimportação, empacotamento ou geração assistida de um arquivo não prova sua autoria artística; por isso não foram fabricados commits de arte atribuídos a Luiza. Cenas e prefabs mistos são mantidos inteiros. Os históricos anteriores continuam sendo a fonte para a contribuição original de cada pessoa.

As diferenças remotas referentes à geração anterior dos mapas foram preservadas no histórico e no inventário. O estado funcional V6 foi usado como fonte dos sistemas atuais, sem introduzir funcionalidades novas da geração anterior em cenas com contratos diferentes.

## Política de sprites

Consultar SPRITE_USAGE_POLICY.md. O relatório strict-sprite-verification.json identifica os arquivos retidos e as cenas que os utilizam. Cenas antigas, prefabs soltos, bibliotecas de revisão e ferramentas antigas de produção não são prova de uso no jogo atual. Comandos legados que reconstruíam mapas a partir dessas fontes aposentadas não devem ser executados na base oficial.
