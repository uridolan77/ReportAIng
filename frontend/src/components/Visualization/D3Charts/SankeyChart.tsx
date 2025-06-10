import React, { useEffect, useRef, useState } from 'react';
import * as d3 from 'd3';
import { sankey, sankeyLinkHorizontal } from 'd3-sankey';
import { useComponentSize } from '../../../hooks/usePerformance';
import { useAccessibility } from '../../../hooks/useAccessibility';

interface SankeyData {
  nodes: Array<{ id?: string; name: string; category?: string; description?: string }>;
  links: Array<{ source: string | number; target: string | number; value: number; label?: string }>;
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

    // Prepare data for sankey - d3-sankey expects specific format
    const sankeyData = {
      nodes: data.nodes.map((d, i) => ({
        ...d,
        id: d.id || i,
        nodeId: i
      })),
      links: data.links.map(d => ({
        ...d,
        source: typeof d.source === 'string' ?
          data.nodes.findIndex(n => n.name === d.source || n.id === d.source) :
          d.source,
        target: typeof d.target === 'string' ?
          data.nodes.findIndex(n => n.name === d.target || n.id === d.target) :
          d.target
      }))
    };

    // Create sankey layout
    const sankeyLayout = sankey()
      .nodeWidth(nodeWidth)
      .nodePadding(nodePadding)
      .extent([[0, 0], [innerWidth, innerHeight]]);

    const { nodes, links } = sankeyLayout(sankeyData as any);

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
      .attr('stroke', (d: any) => {
        const sourceNode = d.source as any;
        return colorScale(sourceNode.category || sourceNode.name);
      })
      .attr('stroke-width', (d: any) => Math.max(1, d.width || 0))
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
      .attr('x', (d: any) => d.x0 || 0)
      .attr('y', (d: any) => d.y0 || 0)
      .attr('width', (d: any) => (d.x1 || 0) - (d.x0 || 0))
      .attr('height', (d: any) => (d.y1 || 0) - (d.y0 || 0))
      .attr('fill', (d: any) => colorScale(d.category || d.name))
      .attr('stroke', '#000')
      .attr('stroke-width', 1)
      .style('cursor', interactive ? 'pointer' : 'default');

    // Add node labels
    g.append('g')
      .attr('class', 'node-labels')
      .selectAll('.node-label')
      .data(nodes)
      .enter()
      .append('text')
      .attr('class', 'node-label')
      .attr('x', (d: any) => {
        const x = (d.x0 || 0) + ((d.x1 || 0) - (d.x0 || 0)) / 2;
        return x < innerWidth / 2 ? (d.x1 || 0) + 6 : (d.x0 || 0) - 6;
      })
      .attr('y', (d: any) => ((d.y0 || 0) + (d.y1 || 0)) / 2)
      .attr('text-anchor', (d: any) => {
        const x = (d.x0 || 0) + ((d.x1 || 0) - (d.x0 || 0)) / 2;
        return x < innerWidth / 2 ? 'start' : 'end';
      })
      .attr('dominant-baseline', 'middle')
      .style('font-size', '12px')
      .style('font-weight', 'bold')
      .text((d: any) => d.name);

    // Add value labels to nodes if enabled
    if (showValues) {
      g.append('g')
        .attr('class', 'node-values')
        .selectAll('.node-value')
        .data(nodes)
        .enter()
        .append('text')
        .attr('class', 'node-value')
        .attr('x', (d: any) => {
          const x = (d.x0 || 0) + ((d.x1 || 0) - (d.x0 || 0)) / 2;
          return x < innerWidth / 2 ? (d.x1 || 0) + 6 : (d.x0 || 0) - 6;
        })
        .attr('y', (d: any) => ((d.y0 || 0) + (d.y1 || 0)) / 2 + 15)
        .attr('text-anchor', (d: any) => {
          const x = (d.x0 || 0) + ((d.x1 || 0) - (d.x0 || 0)) / 2;
          return x < innerWidth / 2 ? 'start' : 'end';
        })
        .attr('dominant-baseline', 'middle')
        .style('font-size', '10px')
        .style('fill', '#666')
        .text((d: any) => (d.value || 0).toLocaleString());
    }

    // Add interactions
    if (interactive) {
      // Node interactions
      nodeElements
        .on('mouseover', function(event, d) {
          d3.select(this).attr('opacity', 0.8);

          // Highlight connected links
          linkElements
            .attr('opacity', (link: any) => {
              const isConnected = link.source === d || link.target === d;
              return isConnected ? 0.8 : 0.1;
            });

          tooltip.transition()
            .duration(200)
            .style('opacity', 0.9);

          const totalIn = (d as any).targetLinks?.reduce((sum: number, link: any) => sum + (link.value || 0), 0) || 0;
          const totalOut = (d as any).sourceLinks?.reduce((sum: number, link: any) => sum + (link.value || 0), 0) || 0;

          tooltip.html(`
            <strong>${(d as any).name}</strong><br/>
            ${(d as any).description ? `${(d as any).description}<br/>` : ''}
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
          onNodeClick?.(d as any);
          announce(`Selected node: ${(d as any).name} with total value ${((d as any).value || 0).toLocaleString()}`);
        });

      // Link interactions
      linkElements
        .on('mouseover', function(event, d) {
          d3.select(this)
            .attr('opacity', 0.8)
            .attr('stroke-width', Math.max(2, ((d as any).width || 0) + 2));

          tooltip.transition()
            .duration(200)
            .style('opacity', 0.9);

          const sourceNode = (d as any).source as any;
          const targetNode = (d as any).target as any;

          tooltip.html(`
            <strong>${sourceNode.name} → ${targetNode.name}</strong><br/>
            ${(d as any).label ? `${(d as any).label}<br/>` : ''}
            Value: ${((d as any).value || 0).toLocaleString()}
          `)
            .style('left', (event.pageX + 10) + 'px')
            .style('top', (event.pageY - 28) + 'px');
        })
        .on('mouseout', function(event, d) {
          d3.select(this)
            .attr('opacity', linkOpacity)
            .attr('stroke-width', Math.max(1, (d as any).width || 0));

          tooltip.transition()
            .duration(500)
            .style('opacity', 0);
        })
        .on('click', function(event, d) {
          setSelectedElement({ type: 'link', data: d });
          onLinkClick?.(d as any);
          const sourceNode = (d as any).source as any;
          const targetNode = (d as any).target as any;
          announce(`Selected flow: ${sourceNode.name} to ${targetNode.name} with value ${((d as any).value || 0).toLocaleString()}`);
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

  }, [data, size, config, onNodeClick, onLinkClick, announce, title, colorScheme, interactive, nodeWidth, nodePadding, linkOpacity, showValues]);

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
