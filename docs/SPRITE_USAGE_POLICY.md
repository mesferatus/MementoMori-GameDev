# Política de sprites do jogo

Manter apenas texturas Sprite com uso comprovado no jogo atual. Uma intenção genérica de uso futuro não é prova de utilização.

O fechamento considera as cinco cenas habilitadas no build e suas dependências recursivas, incluindo prefabs realmente referenciados, materiais, animações e controllers. Também inclui assets configurados no projeto e os carregamentos dinâmicos identificados no código atual: retratos/dados de diálogo, fontes TMP e a folha Resources/UI/Grimoire/GRIMOIRE_UI_SHEET_V2 utilizada por GrimoireScreen.Sprite.

Uma folha/atlas que contém sprites efetivamente usados permanece inteira; esta etapa não redesenha nem recorta novas folhas. Prefabs soltos, cenas antigas e ferramentas históricas de produção não justificam manter imagens no projeto oficial.

Resultado da limpeza: 1.272 texturas importadas como Sprite antes; 457 removidas; 815 restantes, todas no fechamento comprovado. Outros 116 assets serializados antigos dependentes foram arquivados para evitar referências quebradas. As cinco cenas oficiais mantêm os mesmos hashes da V6 verificada.

Provas locais: C:/Projetos/MementoMori-Consolidacao-20261026/Auditoria/strict-sprite-audit.json, strict-sprite-removal-plan.json, strict-sprite-removal-result.json e strict-sprite-verification.json. Recuperação: pacote Quarentena/StrictSpriteCleanup/retired-sprites-and-dependents.unitypackage e backups SHA-256 separados.

Ao adicionar um novo carregamento dinâmico, registrar o código e o caminho utilizados. Revalidar sprites, referências e testes depois de novas limpezas.
