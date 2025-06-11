/**
 * Modern Data Display Components
 * 
 * Provides comprehensive data display components including tables, lists, trees, and more.
 */

import React, { forwardRef } from 'react';
import { Table as AntTable, List as AntList, Tree as AntTree, Tag as AntTag, Badge as AntBadge, Avatar as AntAvatar, Statistic as AntStatistic } from 'antd';
import { designTokens } from './design-system';

// Basic exports - these will be expanded later
export const Table = forwardRef<any, any>((props, ref) => <AntTable ref={ref} {...props} />);
export const List = forwardRef<any, any>((props, ref) => <AntList ref={ref} {...props} />);
export const Tree = forwardRef<any, any>((props, ref) => <AntTree ref={ref} {...props} />);
export const Tag = forwardRef<any, any>((props, ref) => <AntTag ref={ref} {...props} />);
export const Badge = forwardRef<any, any>((props, ref) => <AntBadge ref={ref} {...props} />);
export const Avatar = forwardRef<any, any>((props, ref) => <AntAvatar ref={ref} {...props} />);
export const Statistic = forwardRef<any, any>((props, ref) => <AntStatistic ref={ref} {...props} />);
