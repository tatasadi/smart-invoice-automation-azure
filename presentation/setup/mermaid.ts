/* ./setup/mermaid.ts */
import { defineMermaidSetup } from '@slidev/types'

export default defineMermaidSetup(() => {
  return {
    theme: 'dark',
    themeVariables: {
      primaryColor: '#1a1a1a',
      primaryTextColor: '#ffffff',
      primaryBorderColor: '#0078d4',
      lineColor: '#00bcf2',
      secondaryColor: '#1a1a1a',
      secondaryTextColor: '#ffffff',
      tertiaryColor: '#1a1a1a',
      tertiaryTextColor: '#ffffff',
      fontSize: '18px',
      fontFamily: 'Inter, sans-serif',
      nodeBorder: '#0078d4',
      mainBkg: '#1a1a1a',
      textColor: '#ffffff',
      labelTextColor: '#ffffff',
    },
    flowchart: {
      nodeSpacing: 80,
      rankSpacing: 80,
      padding: 20,
      useMaxWidth: true,
    },
  }
})
