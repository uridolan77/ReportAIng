/**
 * Storybook Main Configuration
 * 
 * Comprehensive Storybook setup for component library documentation
 * with TypeScript, CSS support, and modern React features.
 */

import type { StorybookConfig } from '@storybook/react-vite';
import { mergeConfig } from 'vite';

const config: StorybookConfig = {
  stories: [
    '../src/**/*.stories.@(js|jsx|ts|tsx|mdx)',
    '../src/**/*.story.@(js|jsx|ts|tsx)',
    '../docs/**/*.stories.@(js|jsx|ts|tsx|mdx)',
  ],
  
  addons: [
    '@storybook/addon-essentials',
    '@storybook/addon-interactions',
    '@storybook/addon-links',
    '@storybook/addon-docs',
    '@storybook/addon-controls',
    '@storybook/addon-viewport',
    '@storybook/addon-backgrounds',
    '@storybook/addon-measure',
    '@storybook/addon-outline',
    '@storybook/addon-a11y',
    '@storybook/addon-design-tokens',
  ],
  
  framework: {
    name: '@storybook/react-vite',
    options: {},
  },
  
  typescript: {
    check: false,
    reactDocgen: 'react-docgen-typescript',
    reactDocgenTypescriptOptions: {
      shouldExtractLiteralValuesFromEnum: true,
      propFilter: (prop) => (prop.parent ? !/node_modules/.test(prop.parent.fileName) : true),
    },
  },
  
  features: {
    buildStoriesJson: true,
    storyStoreV7: true,
  },
  
  docs: {
    autodocs: 'tag',
    defaultName: 'Documentation',
  },
  
  async viteFinal(config) {
    return mergeConfig(config, {
      define: {
        global: 'globalThis',
      },
      resolve: {
        alias: {
          '@': '/src',
          '@components': '/src/components',
          '@pages': '/src/pages',
          '@hooks': '/src/hooks',
          '@stores': '/src/stores',
          '@utils': '/src/utils',
          '@test-utils': '/src/test-utils',
        },
      },
      css: {
        preprocessorOptions: {
          scss: {
            additionalData: `@import "/src/styles/variables.scss";`,
          },
        },
      },
    });
  },
  
  staticDirs: ['../public'],
  
  core: {
    disableTelemetry: true,
  },
};

export default config;
