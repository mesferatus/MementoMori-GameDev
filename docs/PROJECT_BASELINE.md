+# Baseline do projeto

## VERIFIED_READY

- Estrutura Unity reconhecível: `Assets`, `Packages` e `ProjectSettings`.
- Código, cenas, sistemas de gameplay, testes e áudio presentes na cópia de trabalho.
- Compile: PASS.
- EditMode: 12/12.
- PlayMode: 3/3.
- Smoke/CT evidence: 11/11.
- Os 11 packs LOCAL_ONLY e seus arquivos `.meta` estão protegidos pelo `.gitignore`; o dry-run Git não incluiu nenhum deles.
- O Gothic Castle foi removido por decisão de Callisto.

## EXISTS_NEEDS_REVALIDATION

- Validação visual, playthrough humano, câmera, colisores, UI, iluminação, sorting e ausência de softlock.
- Build Windows reproduzível fora do Editor.
- Aprovação acadêmica da versão Unity: `UNITY_LTS_APPROVAL_PENDING=YES`.

## NOT_READY

- Aprovação humana/visual do fluxo completo.
- Publicação GitHub e exclusão do projeto antigo, condicionadas aos gates finais.

## AUDIT UPDATE 2026-08-19

- Compile reexecutado com URP oficial 17.4.0: PASS.
- EditMode executado com XML: 12 descobertos, 12 aprovados, 0 falhos.
- PlayMode executado com XML: 3 descobertos, 3 aprovados, 0 falhos.
- Smoke/CT evidence reexecutado após o cleanup: 11 critérios, 11 aprovados, 0 falhos.
- Quarto: dois SpriteRenderers desabilitados e exclusivamente visuais foram removidos com backup; referências quebradas atuais: 0.
- Os artefatos Andrealphus/Poe revisados não têm dependência de runtime; permanecem `LEGACY_VISUAL_NOT_MIGRATED`.

