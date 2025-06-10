import React, { useEffect, useRef, useState } from 'react';
import * as d3 from 'd3';
import { useComponentSize } from '../../../hooks/usePerformance';
import { useAccessibility } from '../../../hooks/useAccessibility';

interface TreemapData {
  name: string;
  value?: number;
  children?: TreemapData[];
  category?: string;
  description?: string;
}

interface TreemapChartProps {
  data: TreemapData;
  config: {
    title?: string;
    colorScheme?: string[];
    showLabels?: boolean;
    interactive?: boolean;
    padding?: number;
    minFontSize?: number;
    maxFontSize?: number;
  };
  onNodeClick?: (data: TreemapData) => void;
  onNodeHover?: (data: TreemapData | null) => void;
}

export const TreemapChart: React.FC<TreemapChartProps> = ({
  data,
  config = {},
  onNodeClick,
  onNodeHover
}) => {
  const svgRef = useRef<SVGSVGElement>(null);
  const tooltipRef = useRef<HTMLDivElement>(null);
  const { elementRef, size } = useComponentSize();
  const [selectedNode, setSelectedNode] = useState<TreemapData | null>(null);
  const { announce } = useAccessibility();

  const {
    title = 'Treemap',
    colorScheme = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', '#9467bd', '#8c564b'],
    showLabels = true,
    interactive = true,
    padding = 2,
    minFontSize = 10,
    maxFontSize = 24
  } = config;

  useEffect(() => {
    if (!svgRef.current || !data || size.width === 0) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll('*').remove();

    const width = size.width;
    const height = Math.max(400, size.width * 0.6);

    svg.attr('width', width).attr('height', height);

    // Create hierarchy
    const root = d3.hierarchy(data)
      .sum(d => d.value || 0)
      .sort((a, b) => (b.value || 0) - (a.value || 0));

    // Create treemap layout
    const treemap = d3.treemap<TreemapData>()
      .size([width, height])
      .padding(padding)
      .round(true);

    treemap(root);

    // Color scale
    const colorScale = d3.scaleOrdinal(colorScheme);

    // Create tooltip
    const tooltip = d3.select(tooltipRef.current)
      .style('opacity', 0)
      .style('position', 'absolute')
      .style('background', 'rgba(0, 0, 0, 0.8)')
      .style('color', 'white')
      .style('padding', '8px 12px')
      .style('border-radius', '4px')
      .style('font-size', '12px')
      .style('pointer-events', 'none')
      .style('z-index', '1000')
      .style('max-width', '200px');

    // Draw nodes
    const nodes = svg.selectAll('.treemap-node')
      .data(root.leaves())
      .enter()
      .append('g')
      .attr('class', 'treemap-node')
      .attr('transform', (d: any) => `translate(${(d as any).x0},${(d as any).y0})`);

    // Add rectangles
    const rects = nodes.append('rect')
      .attr('width', (d: any) => Math.max(0, (d as any).x1 - (d as any).x0))
      .attr('height', (d: any) => Math.max(0, (d as any).y1 - (d as any).y0))
      .attr('fill', d => {
        const category = d.data.category || d.parent?.data.name || d.data.name;
        return colorScale(category);
      })
      .attr('stroke', '#fff')
      .attr('stroke-width', 1)
      .style('cursor', interactive ? 'pointer' : 'default');

    // Add hover effects and interactions
    if (interactive) {
      rects
        .on('mouseover', function(event, d) {
          d3.select(this)
            .attr('stroke-width', 2)
            .attr('stroke', '#333')
            .style('opacity', 0.8);

          tooltip.transition()
            .duration(200)
            .style('opacity', 0.9);

          const percentage = ((d.value || 0) / (root.value || 1) * 100).toFixed(1);
          tooltip.html(`
            <strong>${d.data.name}</strong><br/>
            Value: ${(d.value || 0).toLocaleString()}<br/>
            Percentage: ${percentage}%
            ${d.data.description ? `<br/><em>${d.data.description}</em>` : ''}
          `)
            .style('left', (event.pageX + 10) + 'px')
            .style('top', (event.pageY - 28) + 'px');

          onNodeHover?.(d.data);
        })
        .on('mouseout', function(event, d) {
          d3.select(this)
            .attr('stroke-width', 1)
            .attr('stroke', '#fff')
            .style('opacity', 1);

          tooltip.transition()
            .duration(500)
            .style('opacity', 0);

          onNodeHover?.(null);
        })
        .on('click', function(event, d) {
          setSelectedNode(d.data);
          onNodeClick?.(d.data);
          announce(`Selected: ${d.data.name} with value ${(d.value || 0).toLocaleString()}`);
        });
    }

    // Add labels
    if (showLabels) {
      nodes.append('text')
        .attr('class', 'treemap-label')
        .selectAll('tspan')
        .data((d: any) => {
          const width = (d as any).x1 - (d as any).x0;
          const height = (d as any).y1 - (d as any).y0;
          const area = width * height;

          // Calculate font size based on area
          const fontSize = Math.max(
            minFontSize,
            Math.min(maxFontSize, Math.sqrt(area) / 8)
          );

          // Only show label if there's enough space
          if (width < fontSize * 2 || height < fontSize) return [];

          // Split long names into multiple lines
          const words = d.data.name.split(' ');
          const lines: string[] = [];
          let currentLine = '';

          for (const word of words) {
            const testLine = currentLine ? `${currentLine} ${word}` : word;
            if (testLine.length * fontSize * 0.6 > width && currentLine) {
              lines.push(currentLine);
              currentLine = word;
            } else {
              currentLine = testLine;
            }
          }
          if (currentLine) lines.push(currentLine);

          return lines.slice(0, Math.floor(height / fontSize));
        })
        .enter()
        .append('tspan')
        .attr('x', function(d) {
          const node = d3.select((this as any).parentNode).datum() as any;
          return (node.x1 - node.x0) / 2;
        })
        .attr('y', function(d, i) {
          const node = d3.select((this as any).parentNode).datum() as any;
          const fontSize = Math.max(
            minFontSize,
            Math.min(maxFontSize, Math.sqrt((node.x1 - node.x0) * (node.y1 - node.y0)) / 8)
          );
          return (node.y1 - node.y0) / 2 + (i - 0.5) * fontSize;
        })
        .attr('text-anchor', 'middle')
        .attr('dominant-baseline', 'middle')
        .style('font-size', function(d) {
          const node = d3.select((this as any).parentNode).datum() as any;
          const area = (node.x1 - node.x0) * (node.y1 - node.y0);
          return `${Math.max(minFontSize, Math.min(maxFontSize, Math.sqrt(area) / 8))}px`;
        })
        .style('font-weight', 'bold')
        .style('fill', function() {
          const bgColor = d3.select((this as any).parentNode).select('rect').attr('fill');
          return d3.hsl(bgColor).l > 0.5 ? '#000' : '#fff';
        })
        .style('pointer-events', 'none')
        .text(d => d);
    }

    // Add title
    if (title) {
      svg.append('text')
        .attr('x', width / 2)
        .attr('y', 20)
        .attr('text-anchor', 'middle')
        .style('font-size', '16px')
        .style('font-weight', 'bold')
        .text(title);
    }

    // Add legend
    const categories = Array.from(new Set(
      root.leaves().map(d => d.data.category || d.parent?.data.name || d.data.name)
    ));

    const legend = svg.append('g')
      .attr('class', 'legend')
      .attr('transform', `translate(10, ${height - categories.length * 20 - 10})`);

    const legendItems = legend.selectAll('.legend-item')
      .data(categories)
      .enter()
      .append('g')
      .attr('class', 'legend-item')
      .attr('transform', (d, i) => `translate(0, ${i * 20})`);

    legendItems.append('rect')
      .attr('width', 15)
      .attr('height', 15)
      .attr('fill', d => colorScale(d));

    legendItems.append('text')
      .attr('x', 20)
      .attr('y', 12)
      .style('font-size', '12px')
      .text(d => d);

  }, [data, size, config, interactive, onNodeClick, onNodeHover, announce, title, colorScheme, showLabels, padding, minFontSize, maxFontSize]);

  return (
    <div ref={elementRef} style={{ width: '100%', position: 'relative' }}>
      <svg
        ref={svgRef}
        role="img"
        aria-label={`${title} treemap visualization`}
        style={{ width: '100%', height: 'auto' }}
      />
      <div ref={tooltipRef} />
      {selectedNode && (
        <div style={{ marginTop: '10px', padding: '8px', background: '#f5f5f5', borderRadius: '4px' }}>
          <strong>Selected:</strong> {selectedNode.name} -
          Value: {(selectedNode.value || 0).toLocaleString()}
          {selectedNode.description && (
            <div style={{ marginTop: '4px', fontStyle: 'italic' }}>
              {selectedNode.description}
            </div>
          )}
        </div>
      )}
    </div>
  );
};
