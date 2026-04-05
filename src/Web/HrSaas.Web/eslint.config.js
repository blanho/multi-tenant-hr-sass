import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

const featureModules = [
  'auth', 'employees', 'leave', 'tenants', 'billing',
  'notifications', 'roles', 'users', 'files', 'audit', 'dashboard',
]

const crossFeatureRules = featureModules.map((feature) => ({
  files: [`src/features/${feature}/**/*`],
  rules: {
    'no-restricted-imports': ['error', {
      patterns: featureModules
        .filter((f) => f !== feature)
        .map((f) => ({
          group: [`@/features/${f}/*`],
          message: `Import from @/features/${f} barrel (index.ts) instead of reaching into its internals.`,
        })),
    }],
  },
}))

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
  },
  ...crossFeatureRules,
])
