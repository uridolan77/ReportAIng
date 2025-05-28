import React, { useEffect, useRef, useState } from 'react';
import * as d3 from 'd3';
// Note: d3-sankey types might need to be installed separately
// For now, we'll use any types for sankey functions
const sankey = (d3 as any).sankey;
const sankeyLinkHorizontal = (d3 as any).sankeyLinkHorizontal;
import { useComponentSize } from '../../../hooks/usePerformance';
import { useAccessibility } from '../../../hooks/useAccessibility';

interface SankeyData {
  nodes: Array<{ id: string; name: string; category?: string; description?: string }>;
  links: Array<{ source: string; target: string; value: number; label?: string }>;
}

interface SankeyChartProps {
  data: SankeyData;
  config: {
    title?: string;
    colorScheme?: string[];
    interactive?: boolean;
    nodeWidth?: number;
    nodePadding?: number;
    linkOpacity?: number;
    showValues?: boolean;
  };
  onNodeClick?: (node: any) => void;
  onLinkClick?: (link: any) => void;
}

export const SankeyChart: React.FC<SankeyChartProps> = ({
  data,
  config = {},
  onNodeClick,
  onLinkClick
}) => {
  const svgRef = useRef<SVGSVGElement>(null);
  const tooltipRef = useRef<HTMLDivElement>(null);
  const { elementRef, size } = useComponentSize();
  const [selectedElement, setSelectedElement] = useState<any>(null);
  const { announce } = useAccessibility();

  const {
    title = 'Sankey Diagram',
    colorScheme = d3.schemeCategory10,
    interactive = true,
    nodeWidth = 15,
    nodePadding = 10,
    linkOpacity = 0.5,
    showValues = true
  } = config;

  useEffect(() => {
    if (!svgRef.current || !data.nodes.length || size.width === 0) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll('*').remove();

    const width = size.width;
    const height = Math.max(400, size.width * 0.6);
    const margin = { top: 40, right: 40, bottom: 40, left: 40 };
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;

    svg.attr('width', width).attr('height', height);

    const g = svg.append('g')
      .attr('transform', `translate(${margin.left},${margin.top})`);

    // Prepare data for sankey
    const sankeyData = {
      nodes: data.nodes.map(d => ({ ...d })),
      links: data.links.map(d => ({ ...d }))
    };

    // Create sankey layout
    const sankeyLayout = sankey<any, any>()
      .nodeWidth(nodeWidth)
      .nodePadding(nodePadding)
      .extent([[0, 0], [innerWidth, innerHeight]]);

    const { nodes, links } = sankeyLayout(sankeyData);

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

    // Draw links
    const linkElements = g.append('g')
      .attr('class', 'links')
      .selectAll('.link')
      .data(links)
      .enter()
      .append('path')
      .attr('class', 'link')
      .attr('d', sankeyLinkHorizontal())
      .attr('stroke', d => {
        const sourceNode = d.source as any;
        return colorScale(sourceNode.category || sourceNode.name);
      })
      .attr('stroke-width', d => Math.max(1, d.width || 0))
      .attr('fill', 'none')
      .attr('opacity', linkOpacity)
      .style('cursor', interactive ? 'pointer' : 'default');

    // Draw nodes
    const nodeElements = g.append('g')
      .attr('class', 'nodes')
      .selectAll('.node')
      .data(nodes)
      .enter()
      .append('rect')
      .attr('class', 'node')
      .attr('x', d => d.x0 || 0)
      .attr('y', d => d.y0 || 0)
      .attr('width', d => (d.x1 || 0) - (d.x0 || 0))
      .attr('height', d => (d.y1 || 0) - (d.y0 || 0))
      .attr('fill', d => colorScale(d.category || d.name))
      .attr('stroke', '#000')
      .attr('stroke-width', 1)
      .style('cursor', interactive ? 'pointer' : 'default');

    // Add node labels
    const nodeLabels = g.append('g')
      .attr('class', 'node-labels')
      .selectAll('.node-label')
      .data(nodes)
      .enter()
      .append('text')
      .attr('class', 'node-label')
      .attr('x', d => {
        const x = (d.x0 || 0) + ((d.x1 || 0) - (d.x0 || 0)) / 2;
        return x < innerWidth / 2 ? (d.x1 || 0) + 6 : (d.x0 || 0) - 6;
      })
      .attr('y', d => ((d.y0 || 0) + (d.y1 || 0)) / 2)
      .attr('text-anchor', d => {
        const x = (d.x0 || 0) + ((d.x1 || 0) - (d.x0 || 0)) / 2;
        return x < innerWidth / 2 ? 'start' : 'end';
      })
      .attr('dominant-baseline', 'middle')
      .style('font-size', '12px')
      .style('font-weight', 'bold')
      .text(d => d.name);

    // Add value labels to nodes if enabled
    if (showValues) {
      g.append('g')
        .attr('class', 'node-values')
        .selectAll('.node-value')
        .data(nodes)
        .enter()
        .append('text')
        .attr('class', 'node-value')
        .attr('x', d => {
          const x = (d.x0 || 0) + ((d.x1 || 0) - (d.x0 || 0)) / 2;
          return x < innerWidth / 2 ? (d.x1 || 0) + 6 : (d.x0 || 0) - 6;
        })
        .attr('y', d => ((d.y0 || 0) + (d.y1 || 0)) / 2 + 15)
        .attr('text-anchor', d => {
          const x = (d.x0 || 0) + ((d.x1 || 0) - (d.x0 || 0)) / 2;
          return x < innerWidth / 2 ? 'start' : 'end';
        })
        .attr('dominant-baseline', 'middle')
        .style('font-size', '10px')
        .style('fill', '#666')
        .text(d => (d.value || 0).toLocaleString());
    }

    // Add interactions
    if (interactive) {
      // Node interactions
      nodeElements
        .on('mouseover', function(event, d) {
          d3.select(this).attr('opacity', 0.8);

          // Highlight connected links
          linkElements
            .attr('opacity', link => {
              const isConnected = link.source === d || link.target === d;
              return isConnected ? 0.8 : 0.1;
            });

          tooltip.transition()
            .duration(200)
            .style('opacity', 0.9);

          const totalIn = d.targetLinks?.reduce((sum, link) => sum + (link.value || 0), 0) || 0;
          const totalOut = d.sourceLinks?.reduce((sum, link) => sum + (link.value || 0), 0) || 0;

          tooltip.html(`
            <strong>${d.name}</strong><br/>
            ${d.description ? `${d.description}<br/>` : ''}
            Total In: ${totalIn.toLocaleString()}<br/>
            Total Out: ${totalOut.toLocaleString()}<br/>
            Net: ${(totalOut - totalIn).toLocaleString()}
          `)
            .style('left', (event.pageX + 10) + 'px')
            .style('top', (event.pageY - 28) + 'px');
        })
        .on('mouseout', function(event, d) {
          d3.select(this).attr('opacity', 1);
          linkElements.attr('opacity', linkOpacity);

          tooltip.transition()
            .duration(500)
            .style('opacity', 0);
        })
        .on('click', function(event, d) {
          setSelectedElement({ type: 'node', data: d });
          onNodeClick?.(d);
          announce(`Selected node: ${d.name} with total value ${(d.value || 0).toLocaleString()}`);
        });

      // Link interactions
      linkElements
        .on('mouseover', function(event, d) {
          d3.select(this)
            .attr('opacity', 0.8)
            .attr('stroke-width', Math.max(2, (d.width || 0) + 2));

          tooltip.transition()
            .duration(200)
            .style('opacity', 0.9);

          const sourceNode = d.source as any;
          const targetNode = d.target as any;

          tooltip.html(`
            <strong>${sourceNode.name} → ${targetNode.name}</strong><br/>
            ${d.label ? `${d.label}<br/>` : ''}
            Value: ${(d.value || 0).toLocaleString()}
          `)
            .style('left', (event.pageX + 10) + 'px')
            .style('top', (event.pageY - 28) + 'px');
        })
        .on('mouseout', function(event, d) {
          d3.select(this)
            .attr('opacity', linkOpacity)
            .attr('stroke-width', Math.max(1, d.width || 0));

          tooltip.transition()
            .duration(500)
            .style('opacity', 0);
        })
        .on('click', function(event, d) {
          setSelectedElement({ type: 'link', data: d });
          onLinkClick?.(d);
          const sourceNode = d.source as any;
          const targetNode = d.target as any;
          announce(`Selected flow: ${sourceNode.name} to ${targetNode.name} with value ${(d.value || 0).toLocaleString()}`);
        });
    }

    // Add title
    if (title) {
      svg.append('text')
        .attr('x', width / 2)
        .attr('y', 25)
        .attr('text-anchor', 'middle')
        .style('font-size', '16px')
        .style('font-weight', 'bold')
        .text(title);
    }

  }, [data, size, config, interactive, onNodeClick, onLinkClick, announce]);

  return (
    <div ref={elementRef} style={{ width: '100%', position: 'relative' }}>
      <svg
        ref={svgRef}
        role="img"
        aria-label={`${title} showing flow between ${data.nodes.length} nodes`}
        style={{ width: '100%', height: 'auto' }}
      />
      <div ref={tooltipRef} />
      {selectedElement && (
        <div style={{ marginTop: '10px', padding: '8px', background: '#f5f5f5', borderRadius: '4px' }}>
          <strong>Selected {selectedElement.type}:</strong>{' '}
          {selectedElement.type === 'node'
            ? `${selectedElement.data.name} (${(selectedElement.data.value || 0).toLocaleString()})`
            : `${selectedElement.data.source.name} → ${selectedElement.data.target.name} (${(selectedElement.data.value || 0).toLocaleString()})`
          }
        </div>
      )}
    </div>
  );
};
