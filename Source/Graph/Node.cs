// Karel Kroeze
// Node.cs
// 2016-12-28
// #define TRACE_POS
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using static FluffyResearchTree.Constants;

namespace FluffyResearchTree
{
    public class Node
    {
        #region Fields

        protected Vector2 _pos = Vector2.zero;
        protected List<Edge<Node,Node>> _inEdges = new List<Edge<Node, Node>>();
        protected List<Edge<Node,Node>> _outEdges = new List<Edge<Node, Node>>();

        protected const float LabSize = 30f;

        protected const float Offset = 2f;
        
        protected bool _largeLabel;
        
        protected Rect _queueRect,
                     _rect,
                     _labelRect,
                     _costLabelRect,
                     _costIconRect,
                     _iconsRect;

        protected bool _rectsSet;

        protected Vector2 _topLeft = Vector2.zero,
                            _right = Vector2.zero,
                             _left = Vector2.zero;

        #endregion Fields

        #region Properties

        public List<Node> Descendants
        {
            get
            {
                return OutNodes.Concat( OutNodes.SelectMany( n => n.Descendants ) ).ToList();
            }
        }

        public List<Edge<Node, Node>> OutEdges => _outEdges;
        public List<Node> OutNodes => _outEdges.Select( e => e.Out ).ToList();
        public List<Edge<Node, Node>> InEdges => _inEdges;
        public List<Node> InNodes => _inEdges.Select( e => e.In ).ToList();

        public Rect CostIconRect
        {
            get
            {
                if ( !_rectsSet )
                    SetRects();

                return _costIconRect;
            }
        }

        public Rect CostLabelRect
        {
            get
            {
                if ( !_rectsSet )
                    SetRects();

                return _costLabelRect;
            }
        }

        public Color DrawColor => Color.white;

        public Rect IconsRect
        {
            get
            {
                if ( !_rectsSet )
                    SetRects();

                return _iconsRect;
            }
        }

        public Rect LabelRect
        {
            get
            {
                if ( !_rectsSet )
                    SetRects();

                return _labelRect;
            }
        }

        /// <summary>
        /// Middle of left node edge
        /// </summary>
        public Vector2 Left
        {
            get
            {
                if ( !_rectsSet )
                    SetRects();

                return _left;
            }
        }

        /// <summary>
        /// Tag UI Rect
        /// </summary>
        public Rect QueueRect
        {
            get
            {
                if ( !_rectsSet )
                    SetRects();

                return _queueRect;
            }
        }

        /// <summary>
        /// Static UI rect for this node
        /// </summary>
        public Rect Rect
        {
            get
            {
                if ( !_rectsSet )
                    SetRects();

                return _rect;
            }
        }

        /// <summary>
        /// Middle of right node edge
        /// </summary>
        public Vector2 Right
        {
            get
            {
                if ( !_rectsSet )
                    SetRects();

                return _right;
            }
        }

        public Vector2 Center => ( Left + Right ) / 2f;

        protected internal virtual bool SetDepth( int min = 1 )
        {
            // calculate desired position
            var isRoot = InNodes.NullOrEmpty();
            var desired = isRoot ? 1 : InNodes.Max( n => n.X ) + 1;
            var depth = Mathf.Max( desired, min );

            // no change
            if ( depth == X )
                return false;

            // update
            X = depth;
            return true;
        }

        public virtual int X
        {
            get => (int)_pos.x;
            set
            {
                if ( value < 0 )
                    throw new ArgumentOutOfRangeException( nameof( value ) );
                if ( Math.Abs( _pos.x - value ) < Epsilon )
                    return;

                Log.Trace( "\t" + this + " X: " + _pos.x + " -> " + value );
                _pos.x = value;

                // update caches
                _rectsSet = false;
                Tree.Size.x = Tree.Nodes.Max( n => n.X );
                Tree.OrderDirty = true;
            }
        }

        public virtual int Y
        {
            get => (int)_pos.y;
            set
            {
                if ( value < 0 )
                    throw new ArgumentOutOfRangeException( nameof( value ) );
                if ( Math.Abs( _pos.y - value ) < Epsilon )
                    return;

                Log.Trace("\t" + this + " Y: " + _pos.y + " -> " + value );
                _pos.y = value;

                // update caches
                _rectsSet = false;
                Tree.Size.z = Tree.Nodes.Max( n => n.Y );
                Tree.OrderDirty = true;
            }
        }

        public virtual float Yf
        {
            get => _pos.y;
            set
            {
                if (Math.Abs(_pos.y - value) < Epsilon)
                    return;

                _pos.y = value;

                // update caches
                Tree.Size.z = Tree.Nodes.Max(n => n.Y) + 1;
                Tree.OrderDirty = true;
            }
        }

        #endregion Properties

        #region Methods

        public virtual string Label { get; }
        
        /// <summary>
        /// Prints debug information.
        /// </summary>
        public virtual void Debug()
        {
            var text = new StringBuilder();
            text.AppendLine( Label + " (" + X + ", " + Y + "):" );
            text.AppendLine( "- Parents" );
            foreach ( Node parent in InNodes )
            {
                text.AppendLine( "-- " + parent.Label );
            }

            text.AppendLine( "- Children" );
            foreach ( Node child in OutNodes )
            {
                text.AppendLine( "-- " + child.Label );
            }

            text.AppendLine( "" );
            Log.Message( text.ToString() );
        }

        

        public override string ToString()
        {
            return Label + _pos;
        }

        public void SetRects()
        {
            // origin
            _topLeft = new Vector2( ( X - 1 ) * ( NodeSize.x + NodeMargins.x ),
                                    ( Yf - 1 ) * ( NodeSize.y + NodeMargins.y ) );

            // main rect
            _rect = new Rect( _topLeft.x,
                              _topLeft.y,
                              NodeSize.x,
                              NodeSize.y );

            // left and right edges
            _left = new Vector2( _rect.xMin, _rect.yMin + _rect.height / 2f );
            _right = new Vector2( _rect.xMax, _left.y );

            // queue rect
            _queueRect = new Rect( _rect.xMax - LabSize / 2f,
                                   _rect.yMin + ( _rect.height - LabSize ) / 2f,
                                   LabSize,
                                   LabSize );

            // label rect
            _labelRect = new Rect( _rect.xMin + 6f,
                                   _rect.yMin + 3f,
                                   _rect.width * 2f / 3f - 6f,
                                   _rect.height * .5f - 3f );

            // research cost rect
            _costLabelRect = new Rect( _rect.xMin + _rect.width * 2f / 3f,
                                       _rect.yMin + 3f,
                                       _rect.width * 1f / 3f - 16f - 3f,
                                       _rect.height * .5f - 3f );

            // research icon rect
            _costIconRect = new Rect( _costLabelRect.xMax,
                                      _rect.yMin + ( _costLabelRect.height - 16f ) / 2,
                                      16f,
                                      16f );

            // icon container rect
            _iconsRect = new Rect( _rect.xMin,
                                   _rect.yMin + _rect.height * .5f,
                                   _rect.width,
                                   _rect.height * .5f );

            // see if the label is too big
            _largeLabel = Text.CalcHeight( Label, _labelRect.width ) > _labelRect.height;

            // done
            _rectsSet = true;
        }

        #endregion Methods

        public virtual void Draw() { }
    }
}
