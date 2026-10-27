# Guia operacional — base V6 consolidada

A produção e integração artística dos mapas estão encerradas. Não reconstruir mapas ou preencher lacunas com nova arte por iniciativa própria. Preservar Tilemaps, props, colliders, shaders, animações e composição visual aprovados.

As cenas oficiais e seus caminhos estão no README. Labirinto e DominioLua estão em Assets/V65Environments/Scenes. Use as cenas de build reais em validações e atalhos, nunca cópias históricas.

Leia o git status e preserve trabalho local antes de mudanças. Use unity-cli e os MCPs da Unity para Editor, inspeção, testes e build. Selecione a instância pelo caminho do projeto e informe sua porta em todas as chamadas. Não editar manualmente YAML de cenas ou prefabs com Editor aberto. Preserve .meta e GUIDs; use APIs da Unity para operações de assets.

Em tarefas narrativas, consulte os documentos/scripts existentes, preserve a geometria e registre ID, gatilho, estado e dependências. As funcionalidades da V6.5 estão apenas planejadas em V65_PROXIMA_ETAPA.md; implemente-as somente quando solicitadas.

Use skills pertinentes: pixel perfect para diagnóstico de escala/câmera; sprite-editor para importação; UI uGUI/UITK conforme o framework existente; unity-cli para validação. Não gerar arte substituta nem novos mapas para resolver um problema técnico.

Sprites devem cumprir SPRITE_USAGE_POLICY.md: somente uso comprovado nas cenas ou no jogo atual. Preservação genérica para o futuro não justifica manter sprites antigos sem uso. Antes de remover, verificar dependências e carregamento dinâmico, preservar backup e manifesto e usar AssetDatabase.

Após mudanças técnicas, executar compilação e os testes EditMode/PlayMode aplicáveis. Para navegação, distinguir fechamento de referências, alcance em grade, casts nos colliders reais e movimentação em Play Mode. Não apresentar uma sonda com narrativa suspensa como playthrough completo. Não confundir FPS no Editor com desempenho da build.

Relatar arquivos alterados, verificações atuais, limitações e pendências. Commit/push requer autorização do usuário; não usar force push nem reescrever história. Não inventar autoria, datas ou resultados.
