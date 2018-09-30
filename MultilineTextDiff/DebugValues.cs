// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace Atrip.MultilineTextDiff
{
  internal static class DebugValues
  {
    public const string source = @"Lorem ipsum dolor sit amet consectetur adipiscing elit euismod scelerisque
montes dictum morbi, ultricies eros cubilia lectus habitasse hendrerit convallis
velit integer pretium. Nisl fames inceptos feugiat laoreet ut ultrices himenaeos
parturient ac donec montes, nullam dui scelerisque quam venenatis posuere
volutpat habitant primis condimentum enim, varius aliquet iaculis nascetur
penatibus nisi curabitur eu metus taciti. Ultrices dictumst nam conubia dis
malesuada platea ad suspendisse urna nibh vivamus himenaeos mauris suscipit,
taciti velit posuere hac neque aliquet torquent et sodales eget parturient
integer.

Orci vulputate donec eget praesent cum in dictumst malesuada dui lobortis,
tellus porttitor laoreet rhoncus metus vehicula urna venenatis consequat
inceptos congue, luctus mattis auctor nibh suspendisse hendrerit enim nullam
augue. Porta rhoncus congue posuere tristique curabitur hac molestie nibh
etiam, sociis himenaeos aptent natoque curae euismod tincidunt odio hendrerit,
commodo viverra ut litora taciti sem malesuada dictum. Varius non class nostra
sagittis mus duis, vitae parturient faucibus tellus mauris.";

    /*
      Lorem ipsum dolor sit amet consectetur adipiscing elit euismod
      scelerisque montes dictum morbi, ultricies eros cubilia lectus habitasse
      hendrerit convallis velit integer pretium. Nisl fames inceptos feugiat
      laoreet ut ultrices himenaeos parturient ac donec montes, nullam dui
      scelerisque quam venenatis posuere volutpat habitant primis condimentum
      enim, varius aliquet iaculis nascetur penatibus nisi curabitur eu metus
      taciti. Ultrices dictumst nam conubia dis malesuada platea ad
      suspendisse urna nibh vivamus himenaeos mauris suscipit, taciti velit
      posuere hac neque aliquet torquent et sodales eget parturient integer.

      Orci vulputate donec eget praesent cum in dictumst malesuada dui
      lobortis, tellus porttitor laoreet rhoncus metus vehicula urna venenatis
      consequat inceptos congue, luctus mattis auctor nibh suspendisse
      hendrerit enim nullam augue. Porta rhoncus congue posuere tristique
      curabitur hac molestie nibh etiam, sociis himenaeos aptent natoque
      curae euismod tincidunt odio hendrerit, commodo viverra ut litora taciti
      sem malesuada dictum. Varius non class nostra sagittis mus duis, vitae
      parturient faucibus tellus mauris.
    */
    public const string modified = @"Lorem ipsum dolor sit amet consectetur adipiscing elit scelerisque
euismod montes dictum morbi, ultricies eros cubilia lectus habitasse
hendrerit convallis velit integer pretium. Nisl fames inceptos feugiat
laoreet ut ultrices ac donec montes, nullam dui himenaeos parturient
scelerisque quam venenatis posuere volutpat habitant primis condimentum
enim, varius aliquet iaculis nascetur penatibus nisi Faucibus risus
molestie habitant enim curabitur eu metus taciti. Ultrices dictumst nam
conubia dis malesuada platea ad suspendisse urna nibh vivamus himenaeos
mauris suscipit, taciti velit posuere hac neque aliquet torquent et
sodales eget parturient integer.

Orci vulputate donec eget praesent cum in dictumst malesuada dui
lobortis, tellus porttitor laoreet rhoncus metus vehicula urna venenatis
consequat inceptos congue, arcu posuere fringilla habitant auctor
molestie, luctus mattis auctor nibh suspendisse hendrerit enim nullam
augue. Porta rhoncus congue posuere tristique curabitur hac molestie
nibh etiam, sociis himenaeos aptent natoque curae euismod tincidunt odio
hendrerit, commodo viverra ut litora taciti sem malesuada dictum. Varius
non class nostra sagittis mus duis, vitae parturient faucibus tellus
mauris.";
  }
}
