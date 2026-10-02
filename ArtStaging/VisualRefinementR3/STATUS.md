# R3 — preparado, ainda não integrado

## Atualização após a retomada

- O pedido mais recente autoriza regenerar também pisos e paredes do Labirinto e Domínio, substituindo a restrição anterior nesses dois mapas.
- Importados pela Unity MCP: 24 sprites Melantha, 16 decorações e 32 tiles (16 Labirinto + 16 Domínio). Evidências: `TestResults/VisualRebuild/r3-imports.tsv` e `r3-tiles-import.txt`.
- Os seis clips de idle/caminhada passaram a usar R3; caminhada com seis quadros por direção, 10 Hz, 0,6 s. Ainda falta revisão em movimento e compatibilizar a escala das ações antigas M2.
- Helper `Assets/Editor/VisualRefinementR3.cs` criado para ampliação x2 por eixo; NÃO EXECUTADO. Erro CS1929 da enumeração de células corrigido com foreach. Compilação posterior ainda precisa ser confirmada no Editor.
- `PlayerController.cs`: espelhamento lateral é desfeito ao voltar a caminhar verticalmente; mudança ainda sem teste.
- Ações M2 existentes foram vistas e podem ser reaproveitadas; não regenerar apenas por duplicação. Precisam ajuste de escala/pivôs ao novo conjunto de locomoção.
- Usuário está resolvendo a inicialização/Safe Mode da Unity. Não interferir na instância enquanto ele trabalha; aguardar reconexão MCP.
- Nenhum mapa ampliado nesta rodada; decoração R3 ainda não distribuída; nenhuma validação/screenshot/build R3 concluída. Os parágrafos abaixo descrevem a preparação anterior e foram superados onde indicado nesta atualização.

- Pedido vigente: anexo `700d4383-8303-47bf-926c-2cf5c2b7d261/Texto colado.txt`.
- Melantha_R3_Source.png: folha de 24 poses/quadros gerada com image_gen; alpha confirmado nos cantos e intervalos. Precisa aprovação quadro a quadro e teste de movimento.
- Decoration_R3_Source.png: folha de 16 props gerada com image_gen. Inclui estátua, coluna, obelisco, banco, estandarte, candelabro, estrutura funerária, nicho, arbusto, árvore, cristais, fonte, astrolábio, altar, pergaminhos e lanternas.
- slices.tsv: 40 recortes planejados, com limites de linha manuais porque a geração não respeitou exatamente a grade solicitada. Validar recortes antes de aprovar.
- Tools/ImportVisualR3.cs.txt: adaptação do importador R2, somente para execução pela Unity MCP. Não executado. Melantha com escala comum, altura máxima 56 pixels, canvas 64x64, PPU32 e pivô inferior.
- ObjectiveToastController: objetivo persistente por padrão, mesma mensagem pode reaparecer, interface recriada se perdida na troca de cena. Teste PlayMode atualizado para o novo requisito R3. Ainda não compilado/testado nesta rodada.
- Nenhuma cena, clip ou Animator alterado nesta rodada. Nenhum mapa ampliado ainda. Sem nova build e sem screenshots R3.

Bloqueio: Editor ausente no início; tentativas de abertura ficaram presas antes de carregar o projeto. A primeira prendeu o cliente de licença. Após encerrar somente os processos iniciados nesta retomada e reabrir com acesso normal, a licença foi reconhecida, porém o Editor ainda não carregou nem disponibilizou MCP. Última tentativa encerrada para permitir abertura pela Unity Hub. Usuário solicitado a abrir o projeto e resolver eventuais avisos. Última descoberta MCP: zero instâncias.

Próximo: conectar à Unity, compilar e testar objetivo; revisar/importar Melantha, ajustar animações/escala e conferir em movimento; UI/screenshots; ampliar espaços e reposicionar dependências dos puzzles reutilizando tiles; integrar decoração; rodar validação R3. Resultados R2 não são resultados R3.
