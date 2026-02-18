import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

const MyPreset = definePreset(Aura, {
  semantic: {
    colorScheme: {
      light: {
        primitive: {
          primary: {
            500: 'oklch(0.205 0 0)',
            600: 'oklch(0.185 0 0)', // hover darker
            700: 'oklch(0.165 0 0)'  // active darker
          }
        },
        semantic: {
          background: 'oklch(1 0 0)',
          foreground: 'oklch(0.145 0 0)',
          card: 'oklch(1 0 0)',
          cardForeground: 'oklch(0.145 0 0)',
          popover: 'oklch(1 0 0)',
          popoverForeground: 'oklch(0.145 0 0)',

          border: 'oklch(0.922 0 0)',
          input: 'oklch(0.922 0 0)',
          ring: 'oklch(0.708 0 0)',

          highlight: {
            background: '{primary.500}',
            color: 'oklch(0.985 0 0)',
            hoverBackground: '{primary.600}',
            activeBackground: '{primary.700}'
          },

          secondary: {
            background: 'oklch(0.97 0 0)',
            color: 'oklch(0.205 0 0)',
            hoverBackground: 'oklch(0.94 0 0)'
          },

          muted: {
            background: 'oklch(0.97 0 0)',
            color: 'oklch(0.556 0 0)'
          },

          accent: {
            background: 'oklch(0.97 0 0)',
            color: 'oklch(0.205 0 0)'
          },

          danger: {
            background: 'oklch(0.577 0.245 27.325)',
            hoverBackground: 'oklch(0.54 0.245 27.325)',
            color: 'oklch(0.985 0 0)'
          }
        }
      },

      dark: {
        primitive: {
          primary: {
            500: 'oklch(0.922 0 0)',
            600: 'oklch(0.882 0 0)',
            700: 'oklch(0.842 0 0)'
          }
        },
        semantic: {
          background: 'oklch(0.145 0 0)',
          foreground: 'oklch(0.985 0 0)',
          card: 'oklch(0.205 0 0)',
          cardForeground: 'oklch(0.985 0 0)',
          popover: 'oklch(0.205 0 0)',
          popoverForeground: 'oklch(0.985 0 0)',

          border: 'oklch(1 0 0 / 10%)',
          input: 'oklch(1 0 0 / 15%)',
          ring: 'oklch(0.556 0 0)',

          highlight: {
            background: '{primary.500}',
            color: 'oklch(0.205 0 0)',
            hoverBackground: '{primary.600}',
            activeBackground: '{primary.700}'
          },

          secondary: {
            background: 'oklch(0.269 0 0)',
            color: 'oklch(0.985 0 0)',
            hoverBackground: 'oklch(0.3 0 0)'
          },

          muted: {
            background: 'oklch(0.269 0 0)',
            color: 'oklch(0.708 0 0)'
          },

          accent: {
            background: 'oklch(0.269 0 0)',
            color: 'oklch(0.985 0 0)'
          },

          danger: {
            background: 'oklch(0.704 0.191 22.216)',
            hoverBackground: 'oklch(0.66 0.191 22.216)',
            color: 'oklch(0.985 0 0)'
          }
        }
      }
    }
  }
});

export default MyPreset;
