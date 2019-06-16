// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components.RenderTree;

namespace Microsoft.AspNetCore.Components.Rendering
{
    internal class RenderTreeUpdater
    {
        public static void UpdateToMatchClientState(RenderTreeBuilder renderTreeBuilder, int eventHandlerId, string attributeName, object attributeValue)
        {
            // Find the element that contains the event handler
            var frames = renderTreeBuilder.GetFrames();
            var framesArray = frames.Array;
            var framesLength = frames.Count;
            var closestElementFrameIndex = -1;
            for (var frameIndex = 0; frameIndex < framesLength; frameIndex++)
            {
                ref var frame = ref framesArray[frameIndex];
                switch (frame.FrameType)
                {
                    case RenderTreeFrameType.Element:
                        closestElementFrameIndex = frameIndex;
                        break;
                    case RenderTreeFrameType.Attribute:
                        if (frame.AttributeEventHandlerId == eventHandlerId)
                        {
                            UpdateFrameToMatchClientState(renderTreeBuilder, framesArray, closestElementFrameIndex, attributeName, attributeValue);
                            return;
                        }
                        break;
                }
            }
        }

        private static void UpdateFrameToMatchClientState(RenderTreeBuilder renderTreeBuilder, RenderTreeFrame[] framesArray, int elementFrameIndex, string attributeName, object attributeValue)
        {
            // Find the attribute frame
            ref var elementFrame = ref framesArray[elementFrameIndex];
            var elementSubtreeEndIndexExcl = elementFrameIndex + elementFrame.ElementSubtreeLength;
            for (var attributeFrameIndex = elementFrameIndex + 1; attributeFrameIndex < elementSubtreeEndIndexExcl; attributeFrameIndex++)
            {
                ref var attributeFrame = ref framesArray[attributeFrameIndex];
                if (attributeFrame.FrameType != RenderTreeFrameType.Attribute)
                {
                    // We're now looking at the descendants not attributes, so the search is over
                    break;
                }

                if (attributeFrame.AttributeName == attributeName)
                {
                    // Found an existing attribute we can update
                    attributeFrame = attributeFrame.WithAttributeValue(attributeValue);
                    return;
                }
            }

            // If we get here, we didn't find the desired attribute, so we have to insert a new frame for it
            var insertAtIndex = elementFrameIndex + 1;
            renderTreeBuilder.InsertAttributeExpensive(insertAtIndex, attributeName, attributeValue);

            // Update subtree length
            // TODO: Also update ancestors' subtree lengths
            elementFrame = elementFrame.WithElementSubtreeLength(elementFrame.ElementSubtreeLength + 1);
        }
    }
}
