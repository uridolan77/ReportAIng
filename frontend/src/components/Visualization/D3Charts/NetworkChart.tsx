import React, { useEffect, useRef, useState } from 'react';
import * as d3 from 'd3';
import { useComponentSize } from '../../../hooks/usePerformance';
import { useAccessibility } from '../../../hooks/useAccessibility';

interface NetworkNode {
  id: string;
  name: string;
  group?: string;
  size?: number;
  color?: string;
  description?: string;
  x?: number;
  y?: number;
  fx?: number | null;
  fy?: number | null;
}

interface NetworkLink {
  source: string;
  target: string;
  value?: number;
  label?: string;
  color?: string;
}

interface NetworkData {
  nodes: NetworkNode[];
  links: NetworkLink[];
}

interface NetworkChartProps {
  data: NetworkData;
  config: {
    title?: string;
    colorScheme?: string[];
    interactive?: boolean;
    showLabels?: boolean;
    linkDistance?: number;
    chargeStrength?: number;
    nodeRadius?: (d: NetworkNode) => number;
    linkWidth?: (d: NetworkLink) => number;
  };
  onNodeClick?: (node: NetworkNode) => void;
  onLinkClick?: (link: NetworkLink) => void;
}

export const NetworkChart: React.FC<NetworkChartProps> = ({
  data,
  config = {},
  onNodeClick,
  onLinkClick
}) => {
  const svgRef = useRef<SVGSVGElement>(null);
  const tooltipRef = useRef<HTMLDivElement>(null);
  const { elementRef, size } = useComponentSize();
  const [selectedElement, setSelectedElement] = useState<any>(null);
  const [simulation, setSimulation] = useState<d3.Simulation<NetworkNode, NetworkLink> | null>(null);
  const { announce } = useAccessibility();

  const {
    title = 'Network Graph',
    colorScheme = d3.schemeCategory10,
    interactive = true,
    showLabels = true,
    linkDistance = 100,
    chargeStrength = -300,
    nodeRadius = (d: NetworkNode) => Math.sqrt((d.size || 10) * 10),
    linkWidth = (d: NetworkLink) => Math.sqrt(d.value || 1)
  } = config;

  useEffect(() => {
    if (!svgRef.current || !data.nodes.length || size.width === 0) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll('*').remove();

    const width = size.width;
    const height = Math.max(400, size.width * 0.6);

    svg.attr('width', width).attr('height', height);

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

    // Create simulation
    const sim = d3.forceSimulation<NetworkNode>(data.nodes)
      .force('link', d3.forceLink<NetworkNode, NetworkLink>(data.links)
        .id(d => d.id)
        .distance(linkDistance))
      .force('charge', d3.forceManyBody().strength(chargeStrength))
      .force('center', d3.forceCenter(width / 2, height / 2))
      .force('collision', d3.forceCollide().radius(d => nodeRadius(d) + 2));

    setSimulation(sim);

    // Create container groups
    const linkGroup = svg.append('g').attr('class', 'links');
    const nodeGroup = svg.append('g').attr('class', 'nodes');
    const labelGroup = svg.append('g').attr('class', 'labels');

    // Draw links
    const linkElements = linkGroup.selectAll('.link')
      .data(data.links)
      .enter()
      .append('line')
      .attr('class', 'link')
      .attr('stroke', d => d.color || '#999')
      .attr('stroke-width', linkWidth)
      .attr('opacity', 0.6)
      .style('cursor', interactive ? 'pointer' : 'default');

    // Draw nodes
    const nodeElements = nodeGroup.selectAll('.node')
      .data(data.nodes)
      .enter()
      .append('circle')
      .attr('class', 'node')
      .attr('r', nodeRadius)
      .attr('fill', d => d.color || colorScale(d.group || 'default'))
      .attr('stroke', '#fff')
      .attr('stroke-width', 2)
      .style('cursor', interactive ? 'pointer' : 'default');

    // Add labels
    let labelElements: any = null;
    if (showLabels) {
      labelElements = labelGroup.selectAll('.label')
        .data(data.nodes)
        .enter()
        .append('text')
        .attr('class', 'label')
        .attr('text-anchor', 'middle')
        .attr('dominant-baseline', 'middle')
        .style('font-size', '10px')
        .style('font-weight', 'bold')
        .style('fill', '#333')
        .style('pointer-events', 'none')
        .text(d => d.name);
    }

    // Add drag behavior
    if (interactive) {
      const drag = d3.drag<SVGCircleElement, NetworkNode>()
        .on('start', (event, d) => {
          if (!event.active) sim.alphaTarget(0.3).restart();
          d.fx = d.x;
          d.fy = d.y;
        })
        .on('drag', (event, d) => {
          d.fx = event.x;
          d.fy = event.y;
        })
        .on('end', (event, d) => {
          if (!event.active) sim.alphaTarget(0);
          d.fx = null;
          d.fy = null;
        });

      nodeElements.call(drag);

      // Node interactions
      nodeElements
        .on('mouseover', function(event, d) {
          d3.select(this)
            .attr('stroke-width', 4)
            .attr('stroke', '#333');

          // Highlight connected links and nodes
          const connectedNodes = new Set<string>();
          linkElements
            .attr('opacity', (link: any) => {
              const isConnected = link.source.id === d.id || link.target.id === d.id;
              if (isConnected) {
                connectedNodes.add(link.source.id);
                connectedNodes.add(link.target.id);
              }
              return isConnected ? 1 : 0.1;
            })
            .attr('stroke-width', (link: any) => {
              const isConnected = link.source.id === d.id || link.target.id === d.id;
              return isConnected ? linkWidth(link) * 2 : linkWidth(link);
            });

          nodeElements
            .attr('opacity', (node: NetworkNode) => 
              connectedNodes.has(node.id) ? 1 : 0.3
            );

          tooltip.transition()
            .duration(200)
            .style('opacity', 0.9);

          const connections = data.links.filter(link => 
            link.source === d.id || link.target === d.id
          ).length;

          tooltip.html(`
            <strong>${d.name}</strong><br/>
            ${d.description ? `${d.description}<br/>` : ''}
            Group: ${d.group || 'None'}<br/>
            Connections: ${connections}
            ${d.size ? `<br/>Size: ${d.size}` : ''}
          `)
            .style('left', (event.pageX + 10) + 'px')
            .style('top', (event.pageY - 28) + 'px');
        })
        .on('mouseout', function(event, d) {
          d3.select(this)
            .attr('stroke-width', 2)
            .attr('stroke', '#fff');

          linkElements
            .attr('opacity', 0.6)
            .attr('stroke-width', linkWidth);

          nodeElements.attr('opacity', 1);

          tooltip.transition()
            .duration(500)
            .style('opacity', 0);
        })
        .on('click', function(event, d) {
          setSelectedElement({ type: 'node', data: d });
          onNodeClick?.(d);
          announce(`Selected node: ${d.name} with ${data.links.filter(link => 
            link.source === d.id || link.target === d.id
          ).length} connections`);
        });

      // Link interactions
      linkElements
        .on('mouseover', function(event, d: any) {
          d3.select(this)
            .attr('opacity', 1)
            .attr('stroke-width', linkWidth(d) * 2);

          tooltip.transition()
            .duration(200)
            .style('opacity', 0.9);

          tooltip.html(`
            <strong>${d.source.name} ↔ ${d.target.name}</strong><br/>
            ${d.label ? `${d.label}<br/>` : ''}
            ${d.value ? `Value: ${d.value}` : ''}
          `)
            .style('left', (event.pageX + 10) + 'px')
            .style('top', (event.pageY - 28) + 'px');
        })
        .on('mouseout', function(event, d: any) {
          d3.select(this)
            .attr('opacity', 0.6)
            .attr('stroke-width', linkWidth(d));

          tooltip.transition()
            .duration(500)
            .style('opacity', 0);
        })
        .on('click', function(event, d: any) {
          setSelectedElement({ type: 'link', data: d });
          onLinkClick?.(d);
          announce(`Selected connection: ${d.source.name} to ${d.target.name}`);
        });
    }

    // Update positions on simulation tick
    sim.on('tick', () => {
      linkElements
        .attr('x1', (d: any) => d.source.x)
        .attr('y1', (d: any) => d.source.y)
        .attr('x2', (d: any) => d.target.x)
        .attr('y2', (d: any) => d.target.y);

      nodeElements
        .attr('cx', d => d.x || 0)
        .attr('cy', d => d.y || 0);

      if (labelElements) {
        labelElements
          .attr('x', d => d.x || 0)
          .attr('y', d => d.y || 0);
      }
    });

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

    // Add legend for groups
    const groups = Array.from(new Set(data.nodes.map(d => d.group).filter(Boolean)));
    if (groups.length > 0) {
      const legend = svg.append('g')
        .attr('class', 'legend')
        .attr('transform', `translate(10, 40)`);

      const legendItems = legend.selectAll('.legend-item')
        .data(groups)
        .enter()
        .append('g')
        .attr('class', 'legend-item')
        .attr('transform', (d, i) => `translate(0, ${i * 20})`);

      legendItems.append('circle')
        .attr('r', 6)
        .attr('fill', d => colorScale(d!));

      legendItems.append('text')
        .attr('x', 15)
        .attr('y', 4)
        .style('font-size', '12px')
        .text(d => d!);
    }

    return () => {
      sim.stop();
    };

  }, [data, size, config, interactive, onNodeClick, onLinkClick, announce]);

  // Cleanup simulation on unmount
  useEffect(() => {
    return () => {
      if (simulation) {
        simulation.stop();
      }
    };
  }, [simulation]);

  return (
    <div ref={elementRef} style={{ width: '100%', position: 'relative' }}>
      <svg
        ref={svgRef}
        role="img"
        aria-label={`${title} with ${data.nodes.length} nodes and ${data.links.length} connections`}
        style={{ width: '100%', height: 'auto' }}
      />
      <div ref={tooltipRef} />
      {selectedElement && (
        <div style={{ marginTop: '10px', padding: '8px', background: '#f5f5f5', borderRadius: '4px' }}>
          <strong>Selected {selectedElement.type}:</strong>{' '}
          {selectedElement.type === 'node' 
            ? `${selectedElement.data.name} (Group: ${selectedElement.data.group || 'None'})`
            : `${selectedElement.data.source.name} ↔ ${selectedElement.data.target.name}`
          }
        </div>
      )}
    </div>
  );
};
